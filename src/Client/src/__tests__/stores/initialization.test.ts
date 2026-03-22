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

  it('returns not initialized when API reports no config', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: false,
        isAiConfigured: false,
        isDiscordConfigured: false,
        isSuperAdminConfigured: false,
      }),
    })
    vi.stubGlobal('fetch', mockFetch)

    const store = useInitializationStore()
    await store.checkInitializationStatus()

    expect(store.isInitialized).toBe(false)
    expect(mockFetch).toHaveBeenCalledWith('/api/setup/status')
  })

  it('returns initialized when API reports config exists', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: true,
        isAiConfigured: true,
        isDiscordConfigured: true,
        isSuperAdminConfigured: true,
      }),
    }))

    const store = useInitializationStore()
    await store.checkInitializationStatus()

    expect(store.isInitialized).toBe(true)
  })
})
