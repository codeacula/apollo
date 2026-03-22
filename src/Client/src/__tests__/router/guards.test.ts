import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

import { resolveInitializationNavigation } from '../../router'

describe('Router Guards', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('redirects to setup when not initialized', async () => {
    const initStore = {
      isInitialized: null,
      checkInitializationStatus: vi.fn().mockImplementation(async function (this: { isInitialized: boolean | null }) {
        this.isInitialized = false
      }),
    }

    const result = await resolveInitializationNavigation('/dashboard', initStore)

    expect(result).toBe('/setup')
    expect(initStore.checkInitializationStatus).toHaveBeenCalledOnce()
  })

  it('allows navigation when initialized', async () => {
    const initStore = {
      isInitialized: true,
      checkInitializationStatus: vi.fn(),
    }

    const result = await resolveInitializationNavigation('/dashboard', initStore)

    expect(result).toBe(true)
    expect(initStore.checkInitializationStatus).not.toHaveBeenCalled()
  })
})
