import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import { createMemoryHistory, createRouter } from 'vue-router'
import { createPinia, setActivePinia } from 'pinia'

import ConfigurationStatus from '../../components/ConfigurationStatus.vue'
import { subscribeToDashboardUpdates } from '../../services/dashboardRealtime'
import DashboardView from '../../views/DashboardView.vue'

const { stopMock } = vi.hoisted(() => ({
  stopMock: vi.fn().mockResolvedValue(undefined),
}))

vi.mock('../../services/dashboardRealtime', () => ({
  subscribeToDashboardUpdates: vi.fn().mockResolvedValue({
    stop: stopMock,
  }),
}))

const createOverview = (overrides: Record<string, unknown> = {}) => ({
  generatedAtUtc: '2026-03-22T20:15:00Z',
  configuration: {
    isInitialized: true,
    isConfigured: true,
    subsystems: {
      ai: true,
      discord: true,
      superAdmin: true,
    },
  },
  people: {
    total: 3,
    withAccess: 2,
  },
  toDos: {
    active: 7,
    completed: 4,
    createdToday: 2,
  },
  reminders: {
    scheduled: 5,
    dueWithin24Hours: 3,
    sentToday: 1,
    acknowledged: 2,
  },
  conversations: {
    total: 6,
    messagesLast24Hours: 11,
  },
  activity: [
    {
      kind: 'todo_created',
      title: 'To-do created',
      description: 'codeacula added Clean the cauldron',
      occurredOnUtc: '2026-03-22T19:45:00Z',
    },
    {
      kind: 'reminder_sent',
      title: 'Reminder sent',
      description: 'codeacula: Stretch and drink water',
      occurredOnUtc: '2026-03-22T19:10:00Z',
    },
  ],
  ...overrides,
})

describe('DashboardView', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.mocked(subscribeToDashboardUpdates).mockResolvedValue({
      stop: stopMock,
    })
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

  it('loads dashboard overview data from backend endpoint', async () => {
    const mockFetch = vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    })

    vi.stubGlobal('fetch', mockFetch)

    const wrapper = mountDashboard()
    await flushPromises()

    expect(mockFetch).toHaveBeenCalledWith('/api/dashboard/overview')
    wrapper.unmount()
  })

  it('renders summary cards from the overview payload', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    expect(wrapper.text()).toContain('People with Access')
    expect(wrapper.text()).toContain('2/3')
    expect(wrapper.text()).toContain('Active To-Dos')
    expect(wrapper.text()).toContain('7')
    expect(wrapper.text()).toContain('Messages in 24h')
    expect(wrapper.text()).toContain('11')
    wrapper.unmount()
  })

  it('renders recent activity items', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    expect(wrapper.text()).toContain('Recent activity')
    expect(wrapper.text()).toContain('codeacula added Clean the cauldron')
    expect(wrapper.text()).toContain('Stretch and drink water')
    wrapper.unmount()
  })

  it('shows empty activity copy when nothing has happened yet', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview({ activity: [] })),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    expect(wrapper.text()).toContain('Apollo is either napping or freshly initialized')
    wrapper.unmount()
  })

  it('passes configuration status into the status component', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    const configStatusComponent = wrapper.findComponent(ConfigurationStatus)
    expect(configStatusComponent.exists()).toBe(true)
    expect(configStatusComponent.props('isConfigured')).toBe(true)
    expect(configStatusComponent.props('subsystems')).toEqual({
      ai: true,
      discord: true,
      superAdmin: true,
    })
    wrapper.unmount()
  })

  it('shows realtime status when SignalR is available', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    expect(wrapper.text()).toContain('Live via SignalR')
    wrapper.unmount()
  })

  it('falls back to polling when realtime connection is unavailable', async () => {
    vi.mocked(subscribeToDashboardUpdates).mockResolvedValue(null)
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve(createOverview()),
    }))

    const wrapper = mountDashboard()
    await flushPromises()

    expect(wrapper.text()).toContain('Polling every 15s')
    wrapper.unmount()
  })
})
