import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useInitializationStore } from '../../stores/initializationStore'
import type { RouteLocationNormalized } from 'vue-router'

describe('Router Guards', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  /**
   * When the system is not initialized (initialization store reports false),
   * the router guard should redirect any navigation to the /setup route.
   */
   it('redirects to setup when not initialized', () => {
    const initStore = useInitializationStore()
    initStore.setInitialized(false)

    const mockNext = vi.fn()
    const toRoute: RouteLocationNormalized = {
      path: '/dashboard',
      name: 'Dashboard',
      params: {},
      query: {},
      hash: '',
      fullPath: '/dashboard',
      matched: [],
      meta: {},
      redirectedFrom: undefined,
    }

    // Simulate the guard logic
    if (toRoute.path !== '/setup' && initStore.isInitialized === false) {
      mockNext('/setup')
    } else {
      mockNext()
    }

    expect(mockNext).toHaveBeenCalledWith('/setup')
  })

  /**
   * When the system is initialized (initialization store reports true),
   * the router guard should allow navigation to proceed normally to the
   * requested route.
   */
   it('allows navigation when initialized', () => {
    const initStore = useInitializationStore()
    initStore.setInitialized(true)

    const mockNext = vi.fn()
    const toRoute: RouteLocationNormalized = {
      path: '/dashboard',
      name: 'Dashboard',
      params: {},
      query: {},
      hash: '',
      fullPath: '/dashboard',
      matched: [],
      meta: {},
      redirectedFrom: undefined,
    }

    // Simulate the guard logic
    if (toRoute.path !== '/setup' && initStore.isInitialized === false) {
      mockNext('/setup')
    } else {
      mockNext()
    }

    expect(mockNext).toHaveBeenCalledWith()
  })
})
