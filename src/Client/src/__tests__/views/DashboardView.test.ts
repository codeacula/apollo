import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { setActivePinia, createPinia } from 'pinia'

import DashboardView from '../../views/DashboardView.vue'
import ConfigurationStatus from '../../components/ConfigurationStatus.vue'

describe('DashboardView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  const createRouterForTest = () => createRouter({
    history: createMemoryHistory(),
    routes: [
      {
        path: '/dashboard',
        name: 'Dashboard',
        component: DashboardView,
      },
      {
        path: '/setup',
        name: 'Setup',
        component: { template: '<div>Setup</div>' },
      },
    ],
  })

  const mountDashboard = () => mount(DashboardView, {
    global: {
      plugins: [createRouterForTest()],
      components: { ConfigurationStatus },
    },
  })

  it('shows green/ready indicators for configured subsystems', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: true,
        isAiConfigured: true,
        isDiscordConfigured: true,
        isSuperAdminConfigured: true,
      }),
    }))

    const wrapper = mountDashboard()
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    expect(wrapper.findAll('.status-ready').length).toBeGreaterThanOrEqual(3)
  })

  it('shows red/not-configured indicators for missing config', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: false,
        isAiConfigured: false,
        isDiscordConfigured: true,
        isSuperAdminConfigured: false,
      }),
    }))

    const wrapper = mountDashboard()
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    expect(wrapper.findAll('.status-not-configured').length).toBeGreaterThanOrEqual(2)
  })

  it('shows Not configured message with link to setup', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: false,
        isAiConfigured: false,
        isDiscordConfigured: false,
        isSuperAdminConfigured: false,
      }),
    }))

    const wrapper = mountDashboard()
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    expect(wrapper.html()).toContain('Not configured')
    expect(wrapper.find('[data-action="go-to-setup"]').exists()).toBe(true)
  })

  it('loads status data from backend endpoint', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: true,
        isAiConfigured: true,
        isDiscordConfigured: true,
        isSuperAdminConfigured: true,
      }),
    })

    vi.stubGlobal('fetch', mockFetch)

    const wrapper = mountDashboard()
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    expect(mockFetch).toHaveBeenCalledWith('/api/setup/status')
  })

  it('status UI integrates with dashboard', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isInitialized: true,
        isAiConfigured: true,
        isDiscordConfigured: true,
        isSuperAdminConfigured: true,
      }),
    }))

    const wrapper = mountDashboard()
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    const configStatusComponent = wrapper.findComponent(ConfigurationStatus)
    expect(configStatusComponent.exists()).toBe(true)
    expect(configStatusComponent.props('isConfigured')).toBe(true)
    expect(configStatusComponent.props('subsystems')).toEqual({
      ai: true,
      discord: true,
      superAdmin: true,
    })
  })
})
