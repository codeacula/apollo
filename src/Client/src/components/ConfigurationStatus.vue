<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { SubsystemStatus } from '../services/healthApi'

interface Props {
  subsystems: SubsystemStatus
  isConfigured: boolean
}

defineProps<Props>()

const router = useRouter()

const handleGoToSetup = () => {
  router.push('/setup')
}
</script>

<template>
  <div class="configuration-status">
    <div v-if="!isConfigured" class="not-configured-section">
      <h2>System Not Configured</h2>
      <p class="not-configured-message">
        The system is not fully configured yet. Please complete the setup wizard to get started.
      </p>
      <button
        class="btn btn-primary"
        data-action="go-to-setup"
        @click="handleGoToSetup"
      >
        Go to Setup Wizard
      </button>
    </div>

    <div class="subsystems-section">
      <h2>System Status</h2>
      <div class="subsystems-list">
        <!-- AI Subsystem -->
        <div class="subsystem-row">
          <div class="subsystem-indicator">
            <span
              v-if="subsystems.ai"
              class="status-ready"
              aria-label="AI Ready"
            >
              ✓
            </span>
            <span
              v-else
              class="status-not-configured"
              aria-label="AI Not Configured"
            >
              ✗
            </span>
          </div>
          <div class="subsystem-info">
            <span class="subsystem-name">AI</span>
            <span class="subsystem-status">
              {{ subsystems.ai ? 'Ready' : 'Not configured' }}
            </span>
          </div>
        </div>

        <!-- Discord Subsystem -->
        <div class="subsystem-row">
          <div class="subsystem-indicator">
            <span
              v-if="subsystems.discord"
              class="status-ready"
              aria-label="Discord Ready"
            >
              ✓
            </span>
            <span
              v-else
              class="status-not-configured"
              aria-label="Discord Not Configured"
            >
              ✗
            </span>
          </div>
          <div class="subsystem-info">
            <span class="subsystem-name">Discord</span>
            <span class="subsystem-status">
              {{ subsystems.discord ? 'Ready' : 'Not configured' }}
            </span>
          </div>
        </div>

        <!-- SuperAdmin Subsystem -->
        <div class="subsystem-row">
          <div class="subsystem-indicator">
            <span
              v-if="subsystems.superAdmin"
              class="status-ready"
              aria-label="SuperAdmin Ready"
            >
              ✓
            </span>
            <span
              v-else
              class="status-not-configured"
              aria-label="SuperAdmin Not Configured"
            >
              ✗
            </span>
          </div>
          <div class="subsystem-info">
            <span class="subsystem-name">SuperAdmin</span>
            <span class="subsystem-status">
              {{ subsystems.superAdmin ? 'Ready' : 'Not configured' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.configuration-status {
  padding: 2rem;
}

.not-configured-section {
  text-align: center;
  padding: 2rem;
  background-color: #fee;
  border: 1px solid #fcc;
  border-radius: 8px;
}

.not-configured-section h2 {
  margin-top: 0;
  color: #c33;
}

.not-configured-message {
  color: #666;
  margin-bottom: 1.5rem;
}

.subsystems-section h2 {
  margin-top: 0;
  margin-bottom: 1.5rem;
}

.subsystems-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.subsystem-row {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  background-color: #f9f9f9;
}

.subsystem-indicator {
  font-size: 1.5rem;
  font-weight: bold;
  min-width: 2rem;
  display: flex;
  justify-content: center;
}

.status-ready {
  color: #0a0;
}

.status-not-configured {
  color: #c00;
}

.subsystem-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.subsystem-name {
  font-weight: 600;
  color: #333;
}

.subsystem-status {
  font-size: 0.9rem;
  color: #666;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background-color 0.2s;
}

.btn-primary {
  background-color: #0066cc;
  color: white;
}

.btn-primary:hover {
  background-color: #0052a3;
}
</style>
