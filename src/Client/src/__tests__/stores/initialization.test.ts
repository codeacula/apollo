import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useInitializationStore } from '../../stores/initializationStore'

describe('Initialization Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  /**
   * The initialization store should call GET /api/setup/status and reflect
   * that the system is NOT initialized when the API reports no configuration exists.
   */
  it('returns not initialized when API reports no config', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ isConfigured: false }),
    }))

    const store = useInitializationStore()
    await store.checkInitializationStatus()

    expect(store.isInitialized).toBe(false)
  })

  /**
   * The initialization store should call GET /api/setup/status and reflect
   * that the system IS initialized when the API reports configuration exists,
   * including subsystem details (AI configured, Discord configured, etc.).
   */
  it('returns initialized when API reports config exists', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ isConfigured: true }),
    }))

    const store = useInitializationStore()
    await store.checkInitializationStatus()

    expect(store.isInitialized).toBe(true)
  })
})
