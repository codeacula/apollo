<script setup lang="ts">
import { ref, reactive, getCurrentInstance, onMounted } from 'vue'
import { useInitializationStore } from '../stores/initializationStore'
import { submitSetupConfiguration } from '../services/configApi'

interface SetupConfig {
  modelId: string
  endpoint: string
  apiKey: string
  token: string
  publicKey: string
  botName: string
  discordUserId: string
}

const initStore = useInitializationStore()

const currentStep = ref(1)
const isSubmitting = ref(false)
const errorMessage = ref<string | null>(null)

const config = reactive<SetupConfig>({
  modelId: '',
  endpoint: '',
  apiKey: '',
  token: '',
  publicKey: '',
  botName: '',
  discordUserId: '',
})

const emit = defineEmits<{
  'configuration-changed': [config: Partial<SetupConfig>]
  'setup-complete': []
}>()

// Store references to component context for accessing mocked/real router
let componentInstance: any = null
let componentApp: any = null

onMounted(() => {
  const inst = getCurrentInstance()
  componentInstance = inst
  componentApp = inst?.appContext?.app
})

const isStep1Valid = () => {
  return config.modelId.trim().length > 0 &&
         config.endpoint.trim().length > 0 &&
         config.apiKey.trim().length > 0
}

const isStep2Valid = () => {
  return config.token.trim().length > 0 &&
         config.publicKey.trim().length > 0 &&
         config.botName.trim().length > 0
}

const isStep3Valid = () => {
  return config.discordUserId.trim().length > 0
}

const handleNext = () => {
  if (currentStep.value === 1 && !isStep1Valid()) {
    errorMessage.value = 'Please fill in all AI Configuration fields'
    return
  }
  if (currentStep.value === 2 && !isStep2Valid()) {
    errorMessage.value = 'Please fill in all Discord Configuration fields'
    return
  }
  errorMessage.value = null
  if (currentStep.value < 3) {
    currentStep.value++
  }
}

const handleBack = () => {
  errorMessage.value = null
  if (currentStep.value > 1) {
    currentStep.value--
  }
}

const handleSubmit = async () => {
  if (!isStep3Valid()) {
    errorMessage.value = 'Please fill in the SuperAdmin Discord User ID'
    return
  }

  isSubmitting.value = true
  errorMessage.value = null

  try {
    await submitSetupConfiguration(config)
    initStore.setInitialized(true)
    emit('setup-complete')
    
    // Try to find router from various sources:
    // 1. Captured component instance (from onMounted)
    // 2. Current instance proxy
    // 3. Global properties (Vue Test Utils global.mocks injection)
    let routerInstance = null
    
    if (componentInstance?.proxy?.$router) {
      routerInstance = componentInstance.proxy.$router
    } else if (componentApp?.config?.globalProperties?.$router) {
      routerInstance = componentApp.config.globalProperties.$router
    } else {
      // Fallback: try getCurrentInstance again (though it might be null after await)
      const currentInst = getCurrentInstance()
      if (currentInst?.proxy?.$router) {
        routerInstance = currentInst.proxy.$router
      } else if (currentInst?.appContext?.app?.config?.globalProperties?.$router) {
        routerInstance = currentInst.appContext.app.config.globalProperties.$router
      }
    }
    
    if (routerInstance?.push) {
      await routerInstance.push('/dashboard')
    }
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'An error occurred during setup'
  } finally {
    isSubmitting.value = false
  }
}

const handleConfigChange = (field: keyof SetupConfig, value: string) => {
  config[field] = value
  emit('configuration-changed', { [field]: value })
}
</script>

<template>
  <div class="setup-wizard">
    <div class="wizard-header">
      <h2>Apollo Setup Wizard</h2>
      <p class="step-indicator">Step {{ currentStep }} of 3</p>
    </div>

    <div class="wizard-content">
      <!-- Step 1: AI Configuration -->
      <div v-if="currentStep === 1" class="wizard-step">
        <h3>AI Configuration</h3>
        <form @submit.prevent>
          <div class="form-group">
            <label for="modelId">Model ID</label>
            <input
              id="modelId"
              v-model="config.modelId"
              type="text"
              placeholder="e.g., gpt-4"
              @input="(e) => handleConfigChange('modelId', (e.target as HTMLInputElement).value)"
            />
          </div>
          <div class="form-group">
            <label for="endpoint">Endpoint</label>
            <input
              id="endpoint"
              v-model="config.endpoint"
              type="url"
              placeholder="e.g., https://api.openai.com/v1"
              @input="(e) => handleConfigChange('endpoint', (e.target as HTMLInputElement).value)"
            />
          </div>
          <div class="form-group">
            <label for="apiKey">API Key</label>
            <input
              id="apiKey"
              v-model="config.apiKey"
              type="password"
              placeholder="Your API key"
              @input="(e) => handleConfigChange('apiKey', (e.target as HTMLInputElement).value)"
            />
          </div>
        </form>
      </div>

      <!-- Step 2: Discord Configuration -->
      <div v-if="currentStep === 2" class="wizard-step">
        <h3>Discord Configuration</h3>
        <form @submit.prevent>
          <div class="form-group">
            <label for="token">Bot Token</label>
            <input
              id="token"
              v-model="config.token"
              type="password"
              placeholder="Your Discord bot token"
              @input="(e) => handleConfigChange('token', (e.target as HTMLInputElement).value)"
            />
          </div>
          <div class="form-group">
            <label for="publicKey">Public Key</label>
            <input
              id="publicKey"
              v-model="config.publicKey"
              type="text"
              placeholder="Your Discord application public key"
              @input="(e) => handleConfigChange('publicKey', (e.target as HTMLInputElement).value)"
            />
          </div>
          <div class="form-group">
            <label for="botName">Bot Name</label>
            <input
              id="botName"
              v-model="config.botName"
              type="text"
              placeholder="e.g., Apollo"
              @input="(e) => handleConfigChange('botName', (e.target as HTMLInputElement).value)"
            />
          </div>
        </form>
      </div>

      <!-- Step 3: SuperAdmin Designation -->
      <div v-if="currentStep === 3" class="wizard-step">
        <h3>SuperAdmin Designation</h3>
        <form @submit.prevent>
          <div class="form-group">
            <label for="discordUserId">Discord User ID</label>
            <input
              id="discordUserId"
              v-model="config.discordUserId"
              type="text"
              placeholder="Your Discord user ID"
              @input="(e) => handleConfigChange('discordUserId', (e.target as HTMLInputElement).value)"
            />
          </div>
          <p class="help-text">Enter your Discord user ID to designate yourself as SuperAdmin</p>
        </form>
      </div>
    </div>

    <!-- Error Message -->
    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <!-- Navigation Buttons -->
    <div class="wizard-footer">
      <button
        v-if="currentStep > 1"
        @click="handleBack"
        :disabled="isSubmitting"
        class="btn btn-secondary"
      >
        Back
      </button>
      <button
        v-if="currentStep < 3"
        @click="handleNext"
        :disabled="isSubmitting"
        class="btn btn-primary"
      >
        Next
      </button>
      <button
        v-if="currentStep === 3"
        @click="handleSubmit"
        :disabled="isSubmitting"
        class="btn btn-primary"
      >
        {{ isSubmitting ? 'Submitting...' : 'Submit' }}
      </button>
    </div>
  </div>
</template>

<style scoped>
.setup-wizard {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem;
  border: 1px solid #ccc;
  border-radius: 8px;
  background-color: #f9f9f9;
}

.wizard-header {
  text-align: center;
  margin-bottom: 2rem;
}

.wizard-header h2 {
  margin: 0 0 0.5rem 0;
  font-size: 1.75rem;
}

.step-indicator {
  margin: 0;
  color: #666;
  font-size: 0.95rem;
}

.wizard-content {
  min-height: 300px;
  margin-bottom: 2rem;
}

.wizard-step h3 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  font-size: 1.3rem;
  color: #333;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
  box-sizing: border-box;
}

.form-group input:focus {
  outline: none;
  border-color: #0066cc;
  box-shadow: 0 0 0 3px rgba(0, 102, 204, 0.1);
}

.help-text {
  color: #666;
  font-size: 0.9rem;
  margin-top: 0.5rem;
}

.error-message {
  padding: 0.75rem 1rem;
  margin-bottom: 1rem;
  background-color: #fee;
  color: #c33;
  border: 1px solid #fcc;
  border-radius: 4px;
  font-size: 0.95rem;
}

.wizard-footer {
  display: flex;
  gap: 1rem;
  justify-content: flex-end;
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

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #0066cc;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #0052a3;
}

.btn-secondary {
  background-color: #ccc;
  color: #333;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #bbb;
}
</style>
