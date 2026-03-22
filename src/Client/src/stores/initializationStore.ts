import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface InitializationState {
  isInitialized: boolean | null;
  isLoading: boolean;
  error: string | null;
}

export const useInitializationStore = defineStore('initialization', () => {
  const isInitialized = ref<boolean | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  async function checkInitializationStatus(): Promise<void> {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await fetch('/api/configuration/status');

      if (!response.ok) {
        throw new Error(`Failed to fetch status: ${response.statusText}`);
      }

      const data = await response.json();
      isInitialized.value = data.isConfigured === true;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error';
      isInitialized.value = false;
    } finally {
      isLoading.value = false;
    }
  }

  function setInitialized(value: boolean): void {
    isInitialized.value = value;
  }

  return {
    isInitialized,
    isLoading,
    error,
    checkInitializationStatus,
    setInitialized,
  };
});
