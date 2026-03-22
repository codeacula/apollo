import { defineStore } from 'pinia';
import { ref } from 'vue';

import { getSetupStatus } from '../services/healthApi';

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
      const data = await getSetupStatus();
      isInitialized.value = data.isInitialized;
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
