<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ConfigurationStatus from '../components/ConfigurationStatus.vue'
import { getConfigurationStatus } from '../services/healthApi'
import type { ConfigurationStatus as ConfigurationStatusType } from '../services/healthApi'

const isLoading = ref(true)
const error = ref<string | null>(null)
const statusData = ref<ConfigurationStatusType | null>(null)

onMounted(async () => {
  isLoading.value = true
  error.value = null

  try {
    statusData.value = await getConfigurationStatus()
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An error occurred while fetching status'
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <div class="dashboard">
    <h1>Dashboard</h1>

    <div v-if="isLoading" class="loading-state">
      Loading system status...
    </div>

    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
    </div>

    <div v-else-if="statusData">
      <ConfigurationStatus
        :subsystems="statusData.subsystems"
        :is-configured="statusData.isConfigured"
      />
    </div>
  </div>
</template>

<style scoped>
.dashboard {
  padding: 2rem;
}

.loading-state {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.error-state {
  padding: 1rem;
  background-color: #fee;
  color: #c33;
  border: 1px solid #fcc;
  border-radius: 4px;
}
</style>
