<template>
  <div class="settings-container">
    <header class="settings-header">
      <h1><SettingsIcon class="icon" /> Application Settings</h1>
      <p>Manage system configurations and secure tokens.</p>
    </header>

    <div class="settings-grid">
      <!-- Configuration Form -->
      <section class="settings-card">
        <h2><KeyIcon class="icon" /> Update Configuration</h2>
        <form @submit.prevent="saveConfiguration" class="config-form">
          <div class="form-group">
            <label for="config-key">Configuration Key</label>
            <input 
              id="config-key" 
              v-model="newConfig.key" 
              type="text" 
              placeholder="e.g., Discord:Token" 
              required 
            />
            <small>Common keys: Discord:Token, Discord:PublicKey</small>
          </div>
          <div class="form-group">
            <label for="config-value">Value</label>
            <input 
              id="config-value" 
              v-model="newConfig.value" 
              type="password" 
              placeholder="Secure value..." 
              required 
            />
          </div>
          <button type="submit" :disabled="loading" class="btn btn-primary">
            {{ loading ? 'Saving...' : 'Save Configuration' }}
          </button>
        </form>
      </section>

      <!-- Configuration List -->
      <section class="settings-card">
        <h2><ListIcon class="icon" /> Active Configurations</h2>
        <div v-if="configurations.length === 0" class="empty-state">
          No configurations found.
        </div>
        <ul v-else class="config-list">
          <li v-for="config in configurations" :key="config.key" class="config-item">
            <div class="config-info">
              <span class="config-key">{{ config.key }}</span>
              <span class="config-value">••••••••</span>
            </div>
            <button @click="deleteConfiguration(config.key)" class="btn btn-danger btn-sm">
              <TrashIcon class="icon-sm" />
            </button>
          </li>
        </ul>
      </section>
    </div>

    <div v-if="statusMessage" :class="['status-toast', statusType]">
      {{ statusMessage }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { 
  Settings as SettingsIcon, 
  Key as KeyIcon, 
  List as ListIcon, 
  Trash2 as TrashIcon 
} from 'lucide-vue-next';
import { configurationService, type Configuration } from '../services/configurationService';

const configurations = ref<Configuration[]>([]);
const newConfig = ref({ key: '', value: '' });
const loading = ref(false);
const statusMessage = ref('');
const statusType = ref<'success' | 'error'>('success');

const fetchConfigs = async () => {
  try {
    configurations.value = await configurationService.getAll();
  } catch (error) {
    showStatus('Failed to load configurations.', 'error');
  }
};

const saveConfiguration = async () => {
  loading.value = true;
  try {
    await configurationService.set(newConfig.key, newConfig.value);
    showStatus('Configuration saved successfully!', 'success');
    newConfig.value = { key: '', value: '' };
    await fetchConfigs();
  } catch (error) {
    showStatus('Failed to save configuration.', 'error');
  } finally {
    loading.value = false;
  }
};

const deleteConfiguration = async (key: string) => {
  if (!confirm(`Are you sure you want to delete ${key}?`)) return;
  
  try {
    await configurationService.delete(key);
    showStatus('Configuration deleted.', 'success');
    await fetchConfigs();
  } catch (error) {
    showStatus('Failed to delete configuration.', 'error');
  }
};

const showStatus = (msg: string, type: 'success' | 'error') => {
  statusMessage.value = msg;
  statusType.value = type;
  setTimeout(() => {
    statusMessage.value = '';
  }, 3000);
};

onMounted(fetchConfigs);
</script>

<style scoped>
.settings-container {
  max-width: 1000px;
  margin: 2rem auto;
  padding: 0 1rem;
  font-family: system-ui, -apple-system, sans-serif;
}

.settings-header {
  margin-bottom: 2rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 1rem;
}

.settings-header h1 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin: 0;
  color: #2c3e50;
}

.icon {
  width: 24px;
  height: 24px;
}

.icon-sm {
  width: 16px;
  height: 16px;
}

.settings-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
}

@media (max-width: 768px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }
}

.settings-card {
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.05);
  padding: 1.5rem;
  border: 1px solid #f0f0f0;
}

.settings-card h2 {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.25rem;
  margin-top: 0;
  margin-bottom: 1.5rem;
  color: #34495e;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  font-size: 0.9rem;
}

.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-sizing: border-box;
}

.form-group small {
  display: block;
  margin-top: 0.25rem;
  color: #7f8c8d;
  font-size: 0.8rem;
}

.btn {
  padding: 0.75rem 1.25rem;
  border-radius: 4px;
  border: none;
  cursor: pointer;
  font-weight: 600;
  transition: opacity 0.2s;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #3498db;
  color: white;
  width: 100%;
}

.btn-danger {
  background-color: #e74c3c;
  color: white;
}

.btn-sm {
  padding: 0.4rem;
}

.config-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.config-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  border-bottom: 1px solid #f8f9fa;
}

.config-item:last-child {
  border-bottom: none;
}

.config-info {
  display: flex;
  flex-direction: column;
}

.config-key {
  font-weight: 600;
  font-size: 0.95rem;
}

.config-value {
  color: #bdc3c7;
  font-size: 0.8rem;
}

.status-toast {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  padding: 1rem 1.5rem;
  border-radius: 4px;
  color: white;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  animation: slideIn 0.3s ease-out;
}

.success { background-color: #27ae60; }
.error { background-color: #c0392b; }

@keyframes slideIn {
  from { transform: translateX(100%); opacity: 0; }
  to { transform: translateX(0); opacity: 1; }
}
</style>
