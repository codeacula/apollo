<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'

import ConfigurationStatus from '../components/ConfigurationStatus.vue'
import DashboardActivityFeed from '../components/dashboard/DashboardActivityFeed.vue'
import DashboardConnectionBadge from '../components/dashboard/DashboardConnectionBadge.vue'
import DashboardStatCard from '../components/dashboard/DashboardStatCard.vue'
import DashboardWorkloadSnapshot from '../components/dashboard/DashboardWorkloadSnapshot.vue'
import { getDashboardOverview } from '../services/dashboardApi'
import type { DashboardActivityItem, DashboardOverview } from '../services/dashboardApi'
import { subscribeToDashboardUpdates } from '../services/dashboardRealtime'

const isLoading = ref(true)
const error = ref<string | null>(null)
const overview = ref<DashboardOverview | null>(null)
const liveMode = ref<'signalr' | 'polling' | 'reconnecting'>('reconnecting')

const POLLING_INTERVAL_MS = 15000

let realtimeSubscription: Awaited<ReturnType<typeof subscribeToDashboardUpdates>> = null
let pollingHandle: ReturnType<typeof setInterval> | null = null
let pollingRefreshInFlight = false
let pollingFailureCount = 0
let isUnmounting = false
let isMounted = false

const POLLING_MAX_FAILURES = 3

const liveModeLabel = computed(() => {
  switch (liveMode.value) {
    case 'polling':
      return `Polling every ${POLLING_INTERVAL_MS / 1000}s`
    case 'reconnecting':
      return 'Reconnecting...'
    default:
      return 'Live via SignalR'
  }
})

const statCards = computed(() => {
  if (!overview.value) {
    return []
  }

  return [
    {
      label: 'People with Access',
      value: `${overview.value.people.withAccess}/${overview.value.people.total}`,
      tone: 'warm' as const,
    },
    {
      label: 'Active To-Dos',
      value: overview.value.toDos.active,
      tone: 'sun' as const,
    },
    {
      label: 'Reminders Due Soon',
      value: overview.value.reminders.dueWithin24Hours,
      tone: 'mint' as const,
    },
    {
      label: 'Messages in 24h',
      value: overview.value.conversations.messagesLast24Hours,
      tone: 'sky' as const,
    },
  ]
})

const activityItems = computed<DashboardActivityItem[]>(() => overview.value?.activity ?? [])

async function loadOverview(): Promise<void> {
  overview.value = await getDashboardOverview()
  error.value = null
}

function startPolling(): void {
  if (pollingHandle) {
    return
  }

  liveMode.value = 'polling'
  pollingHandle = setInterval(async () => {
    if (pollingRefreshInFlight) {
      return
    }

    pollingRefreshInFlight = true

    try {
      await loadOverview()
      pollingFailureCount = 0
    } catch (err) {
      pollingFailureCount++
      console.warn('Dashboard polling refresh failed.', err)

      if (pollingFailureCount >= POLLING_MAX_FAILURES) {
        error.value = 'Dashboard data could not be refreshed. Check your connection.'
      }
    } finally {
      pollingRefreshInFlight = false
    }
  }, POLLING_INTERVAL_MS)
}

function stopPolling(): void {
  if (!pollingHandle) {
    return
  }

  clearInterval(pollingHandle)
  pollingHandle = null
  pollingRefreshInFlight = false
  pollingFailureCount = 0
}

onMounted(async () => {
  isMounted = true
  isLoading.value = true
  error.value = null

  try {
    await loadOverview()
    const subscription = await subscribeToDashboardUpdates({
      onOverviewUpdated: updatedOverview => {
        overview.value = updatedOverview
      },
      onConnected: () => {
        liveMode.value = 'signalr'
        stopPolling()
      },
      onReconnect: async () => {
        await loadOverview()
      },
      onReconnecting: () => {
        liveMode.value = 'reconnecting'
      },
      onDisconnected: () => {
        if (!isUnmounting) {
          startPolling()
        }
      },
      onError: err => {
        console.warn('Dashboard realtime connection unavailable, switching to polling.', err)
        startPolling()
      },
      onReconnectError: err => {
        console.warn('Dashboard realtime data refresh failed after reconnect.', err)
        // The SignalR connection is still healthy — just reload data, don't downgrade to polling.
        loadOverview().catch(loadErr => console.warn('Dashboard reload after reconnect error failed.', loadErr))
      },
    })

    if (!isMounted) {
      // Component was unmounted while subscribing — tear down immediately.
      subscription?.stop().catch(err => console.warn('Dashboard realtime stop (late unmount) failed.', err))
      return
    }

    realtimeSubscription = subscription

    if (!realtimeSubscription) {
      startPolling()
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'An error occurred while loading the dashboard'
  } finally {
    isLoading.value = false
  }
})

onUnmounted(() => {
  isMounted = false
  isUnmounting = true
  stopPolling()
  realtimeSubscription?.stop().catch(err => console.warn('Dashboard realtime stop failed.', err))
})
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <div>
        <p class="eyebrow">Apollo Observatory</p>
        <h1>See what the cat is up to.</h1>
        <p class="subtitle">A live snapshot of readiness, workload, and recent Apollo activity.</p>
      </div>
      <div v-if="overview" class="header-meta">
        <DashboardConnectionBadge :mode="liveMode" :label="liveModeLabel" />
        <div class="generated-at">
          Updated {{ new Date(overview.generatedAtUtc).toLocaleString() }}
        </div>
      </div>
    </header>

    <div v-if="isLoading" class="loading-state">
      Loading Apollo's current pulse...
    </div>

    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
    </div>

    <template v-else-if="overview">
      <section class="stats-grid">
        <DashboardStatCard
          v-for="card in statCards"
          :key="card.label"
          :label="card.label"
          :value="card.value"
          :tone="card.tone"
        />
      </section>

      <section class="dashboard-grid">
        <article class="panel panel-status">
          <ConfigurationStatus
            :subsystems="overview.configuration.subsystems"
            :is-configured="overview.configuration.isConfigured"
          />
        </article>

        <DashboardWorkloadSnapshot
          :to-dos="overview.toDos"
          :reminders="overview.reminders"
          :conversations="overview.conversations"
        />

        <DashboardActivityFeed :items="activityItems" class="panel-activity" />
      </section>
    </template>
  </div>
</template>

<style scoped>
.dashboard {
  --ink: #2f221b;
  --muted: #75665d;
  --line: rgba(111, 79, 59, 0.18);
  min-height: 100vh;
  padding: 2rem;
  color: var(--ink);
  background:
    radial-gradient(circle at top left, rgba(255, 201, 133, 0.35), transparent 28%),
    radial-gradient(circle at top right, rgba(247, 174, 137, 0.2), transparent 32%),
    linear-gradient(180deg, #fff7ef 0%, #fffdf8 100%);
}

.dashboard-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.75rem;
}

.eyebrow {
  margin: 0 0 0.5rem;
  font-size: 0.78rem;
  font-weight: 700;
  letter-spacing: 0.18em;
  text-transform: uppercase;
  color: #b5653a;
}

h1 {
  margin: 0;
  font-size: clamp(2rem, 4vw, 3.4rem);
  line-height: 0.95;
}

.subtitle,
.generated-at {
  color: var(--muted);
}

.subtitle {
  max-width: 44rem;
  margin: 0.9rem 0 0;
  font-size: 1rem;
}

.header-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.65rem;
}

.generated-at {
  padding: 0.8rem 1rem;
  border: 1px solid var(--line);
  border-radius: 999px;
  background: rgba(255, 253, 249, 0.8);
  white-space: nowrap;
}

.loading-state {
  text-align: center;
  padding: 2rem;
  color: var(--muted);
}

.error-state {
  padding: 1rem;
  background-color: #fff0ee;
  color: #ae3f2f;
  border: 1px solid #f6c8bf;
  border-radius: 1rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1.15fr 0.85fr;
  gap: 1rem;
}

.panel-status {
  grid-column: 1;
}

.panel-activity {
  grid-column: 1 / span 2;
}

@media (max-width: 960px) {
  .stats-grid,
  .dashboard-grid {
    grid-template-columns: 1fr 1fr;
  }

  .panel-activity {
    grid-column: auto;
  }
}

@media (max-width: 720px) {
  .dashboard {
    padding: 1rem;
  }

  .dashboard-header,
  .stats-grid,
  .dashboard-grid {
    grid-template-columns: 1fr;
    display: grid;
  }

  .dashboard-header {
    gap: 0.75rem;
  }

  .header-meta {
    align-items: flex-start;
  }

  .generated-at {
    white-space: normal;
  }
}
</style>

<style>
@import '../assets/panel.css';
</style>
