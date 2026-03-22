import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { setActivePinia, createPinia } from 'pinia'
import DashboardView from '../../views/DashboardView.vue'
import ConfigurationStatus from '../../components/ConfigurationStatus.vue'
import type { ConfigurationStatus as ConfigurationStatusType } from '../../services/healthApi'

describe('DashboardView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  /**
   * Test 1: Dashboard shows green/ready indicators for configured subsystems
   * When all subsystems are configured, the status component should display
   * green/ready indicators (✓ with status-ready class) for each subsystem.
   */
  it('shows green/ready indicators for configured subsystems', async () => {
    const mockStatus: ConfigurationStatusType = {
      isConfigured: true,
      subsystems: {
        ai: true,
        discord: true,
        superAdmin: true,
      },
    }

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockStatus),
    }))

    const router = createRouter({
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

    const wrapper = mount(DashboardView, {
      global: {
        plugins: [router],
        components: { ConfigurationStatus },
      },
    })

    // Wait for the component to finish loading
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    // Check for status-ready indicators
    const readyIndicators = wrapper.findAll('.status-ready')
    expect(readyIndicators.length).toBeGreaterThanOrEqual(3)
  })

  /**
   * Test 2: Dashboard shows red/not-configured indicators for missing config
   * When subsystems are not configured, the status component should display
   * red indicators (✗ with status-not-configured class) for those subsystems.
   */
  it('shows red/not-configured indicators for missing config', async () => {
    const mockStatus: ConfigurationStatusType = {
      isConfigured: false,
      subsystems: {
        ai: false,
        discord: true,
        superAdmin: false,
      },
    }

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockStatus),
    }))

    const router = createRouter({
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

    const wrapper = mount(DashboardView, {
      global: {
        plugins: [router],
        components: { ConfigurationStatus },
      },
    })

    // Wait for the component to finish loading
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    // Check for status-not-configured indicators
    const notConfiguredIndicators = wrapper.findAll('.status-not-configured')
    expect(notConfiguredIndicators.length).toBeGreaterThanOrEqual(2)
  })

  /**
   * Test 3: Dashboard shows "Not configured" message with link to re-run setup wizard
   * When isConfigured is false, the component should display a message and provide
   * a way to navigate to the setup wizard.
   */
  it('shows Not configured message with link to setup', async () => {
    const mockStatus: ConfigurationStatusType = {
      isConfigured: false,
      subsystems: {
        ai: false,
        discord: false,
        superAdmin: false,
      },
    }

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockStatus),
    }))

    const router = createRouter({
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

    const wrapper = mount(DashboardView, {
      global: {
        plugins: [router],
        components: { ConfigurationStatus },
      },
    })

    // Wait for the component to finish loading
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    // Check for "Not configured" message
    const html = wrapper.html()
    expect(html).toContain('Not configured')

    // Check for setup link/button
    const setupButton = wrapper.find('[data-action="go-to-setup"]')
    expect(setupButton.exists()).toBe(true)
  })

  /**
   * Test 4: Status data is loaded from backend configuration/status endpoint
   * The component should call GET /api/configuration/status and use the returned data.
   */
  it('loads status data from backend endpoint', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        isConfigured: true,
        subsystems: {
          ai: true,
          discord: true,
          superAdmin: true,
        },
      }),
    })

    vi.stubGlobal('fetch', mockFetch)

    const router = createRouter({
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

    const wrapper = mount(DashboardView, {
      global: {
        plugins: [router],
        components: { ConfigurationStatus },
      },
    })

    // Wait for the component to finish loading
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    // Verify fetch was called with correct endpoint
    expect(mockFetch).toHaveBeenCalledWith('/api/configuration/status')
  })

  /**
   * Test 5: Status UI integrates with dashboard
   * The DashboardView should mount and render the ConfigurationStatus component
   * with the fetched status data.
   */
  it('status UI integrates with dashboard', async () => {
    const mockStatus: ConfigurationStatusType = {
      isConfigured: true,
      subsystems: {
        ai: true,
        discord: true,
        superAdmin: true,
      },
    }

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(mockStatus),
    }))

    const router = createRouter({
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

    const wrapper = mount(DashboardView, {
      global: {
        plugins: [router],
        components: { ConfigurationStatus },
      },
    })

    // Wait for the component to finish loading
    await wrapper.vm.$nextTick()
    await new Promise(resolve => setTimeout(resolve, 100))

    // Check that ConfigurationStatus component is rendered
    const configStatusComponent = wrapper.findComponent(ConfigurationStatus)
    expect(configStatusComponent.exists()).toBe(true)

    // Verify the component received the correct props
    expect(configStatusComponent.props('isConfigured')).toBe(true)
    expect(configStatusComponent.props('subsystems')).toEqual({
      ai: true,
      discord: true,
      superAdmin: true,
    })
  })
})
