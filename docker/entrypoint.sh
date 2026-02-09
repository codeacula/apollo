#!/bin/bash
# Apollo AI Development Container Entrypoint
# Handles branch checkout, dependency restoration, and OpenCode launch

set -e

cd /workspace

# --- Fix mounted host files (podman rootless can't read them directly) ---

# Copy .gitconfig to a writable location so git can read/write it
if [ -f "$HOME/.gitconfig" ]; then
    cp "$HOME/.gitconfig" "$HOME/.gitconfig-local" 2>/dev/null || true
else
    touch "$HOME/.gitconfig-local"
fi
export GIT_CONFIG_GLOBAL="$HOME/.gitconfig-local"

# Configure git to trust the workspace directory
git config --global --add safe.directory /workspace

# Copy SSH keys to a writable location with correct permissions
if [ -d "$HOME/.ssh" ]; then
    rm -rf "$HOME/.ssh-local"
    cp -r "$HOME/.ssh" "$HOME/.ssh-local" 2>/dev/null || true
    chmod 700 "$HOME/.ssh-local" 2>/dev/null || true
    chmod 600 "$HOME/.ssh-local/"* 2>/dev/null || true
    # Point git at the local copy for SSH operations
    export GIT_SSH_COMMAND="ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $HOME/.ssh-local/id_ed25519 -i $HOME/.ssh-local/id_rsa 2>/dev/null"
fi

# Checkout branch if specified via BRANCH environment variable
if [ -n "$BRANCH" ]; then
    echo "Switching to branch: $BRANCH"
    git fetch origin 2>/dev/null || true
    git checkout "$BRANCH" 2>/dev/null || \
    git checkout -b "$BRANCH" "origin/$BRANCH" 2>/dev/null || \
    echo "Branch checkout failed, continuing on current branch"
fi

# Restore .NET dependencies
if [ -f "Apollo.sln" ]; then
    echo "Restoring .NET dependencies..."
    dotnet restore Apollo.sln || echo "dotnet restore failed"
fi

# Restore npm dependencies for the Vue client
if [ -d "src/Client" ]; then
    echo "Restoring npm dependencies..."
    npm install --prefix src/Client 2>/dev/null || echo "npm install failed"
fi

echo ""
echo "========================================================"
echo "        Apollo AI Development Environment"
echo "========================================================"
echo ""
echo "  Project files are copied into the container."
echo "  Use git to push changes back to the remote."
echo ""
echo "  MCP Servers:"
echo "    - memory (knowledge graph persistence)"
echo "    - sequential-thinking (step-by-step reasoning)"
echo ""
echo "  LSP Servers:"
echo "    - csharp (C# language support)"
echo "    - typescript (TypeScript/JavaScript)"
echo "    - vue (Vue.js single-file components)"
echo ""
echo "  Useful commands:"
echo "    dotnet build Apollo.sln    - Build the solution"
echo "    dotnet test Apollo.sln     - Run tests"
echo "    git push                    - Push changes to remote"
echo "    gh pr create                - Create a pull request"
echo ""
echo "========================================================"
echo ""

# Launch OpenCode
exec opencode
