#!/bin/bash
# Apollo AI Development Environment Launcher
#
# Usage:
#   ./start-dev.sh                    Start on current branch
#   ./start-dev.sh feature/my-branch  Checkout and start on specified branch
#
# Requirements:
#   - Linux or WSL2
#   - Docker or Podman installed
#   - ~/.gitconfig configured with name/email
#   - ~/.ssh/ with SSH keys for git operations
#
# This script will:
#   1. Auto-detect Docker or Podman
#   2. Build the dev container image with current project files
#   3. Start Postgres and Redis
#   4. Launch OpenCode in an interactive container

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BRANCH="${1:-}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

echo_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

echo_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Auto-detect container runtime
if command -v podman &> /dev/null; then
    RUNTIME="podman"
    if command -v podman-compose &> /dev/null; then
        COMPOSE="podman-compose"
    elif podman compose version &> /dev/null; then
        COMPOSE="podman compose"
    else
        echo_error "podman-compose or 'podman compose' plugin required"
        exit 1
    fi
elif command -v docker &> /dev/null; then
    RUNTIME="docker"
    COMPOSE="docker compose"
else
    echo_error "Neither docker nor podman found. Please install one of them."
    exit 1
fi

echo_info "Using container runtime: $RUNTIME"

# Check for required host files
if [ ! -f "$HOME/.gitconfig" ]; then
    echo_warn "~/.gitconfig not found. Git operations may fail."
    echo_warn "Run: git config --global user.name 'Your Name'"
    echo_warn "     git config --global user.email 'your@email.com'"
fi

if [ ! -d "$HOME/.ssh" ]; then
    echo_warn "~/.ssh directory not found. Git push/pull may fail."
fi

# Always rebuild the image to capture current project files.
# Tooling layers are cached, so only the COPY layer is rebuilt.
echo_info "Building apollo-dev image with current project files..."
$RUNTIME build -t apollo-dev:latest -f "$SCRIPT_DIR/docker/Dockerfile.dev" "$SCRIPT_DIR"

# Start dependencies (Postgres and Redis)
echo_info "Starting Postgres and Redis..."
$COMPOSE -f "$SCRIPT_DIR/compose.dev.yaml" up -d pgsql redis

# Wait for Postgres to be ready
echo_info "Waiting for Postgres to be ready..."
for i in {1..30}; do
    if $RUNTIME exec apollo-pgsql pg_isready -U apollo -d apollo_db &> /dev/null; then
        echo_info "Postgres is ready"
        break
    fi
    if [ $i -eq 30 ]; then
        echo_error "Postgres failed to start in time"
        exit 1
    fi
    sleep 1
done

# Display branch info
if [ -n "$BRANCH" ]; then
    echo_info "Will checkout branch: $BRANCH"
else
    echo_info "Using current branch"
fi

# Run the dev container interactively
echo_info "Starting OpenCode..."
echo ""

RUN_ARGS=(
    -it --rm --replace
    --name apollo-dev
    --network apollo-network
    -v "$HOME/.gitconfig:/home/developer/.gitconfig:ro"
    -v "$HOME/.ssh:/home/developer/.ssh:ro"
    -v "apollo-opencode-data:/home/developer/.local/share/opencode"
    -e "BRANCH=$BRANCH"
    -e "ConnectionStrings__Apollo=Host=apollo-pgsql;Database=apollo_db;Username=apollo;Password=apollo"
    -e "ConnectionStrings__Redis=apollo-redis:6379,password=apollo_redis"
    -e "ConnectionStrings__Quartz=Host=apollo-pgsql;Database=apollo_db;Username=apollo;Password=apollo"
)

$RUNTIME run "${RUN_ARGS[@]}" apollo-dev:latest

# Cleanup message
echo ""
echo_info "OpenCode session ended."
echo_info "Postgres and Redis are still running."
echo_info "To stop them: $COMPOSE -f compose.dev.yaml down"
