import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr'

import type { DashboardOverview } from './dashboardApi'

export interface DashboardRealtimeSubscription {
  stop: () => Promise<void>
}

export interface DashboardRealtimeOptions {
  onOverviewUpdated: (overview: DashboardOverview) => void
  onConnected?: () => void
  onReconnect?: () => Promise<void> | void
  onReconnecting?: () => void
  onDisconnected?: (error?: unknown) => void
  onError?: (error: unknown) => void
}

export async function subscribeToDashboardUpdates(
  options: DashboardRealtimeOptions,
): Promise<DashboardRealtimeSubscription | null> {
  const connection = new HubConnectionBuilder()
    .withUrl('/hubs/dashboard')
    .withAutomaticReconnect()
    .build()

  wireDashboardEvents(connection, options)

  try {
    await connection.start()
    options.onConnected?.()
  } catch (error) {
    options.onError?.(error)
    return null
  }

  return {
    stop: async () => {
      await connection.stop()
    },
  }
}

function wireDashboardEvents(connection: HubConnection, options: DashboardRealtimeOptions): void {
  connection.on('DashboardOverviewUpdated', (overview: DashboardOverview) => {
    options.onOverviewUpdated(overview)
  })

  connection.onreconnecting(() => {
    options.onReconnecting?.()
  })

  connection.onreconnected(async () => {
    options.onConnected?.()
    try {
      await options.onReconnect?.()
    } catch (error) {
      options.onError?.(error)
    }
  })

  connection.onclose(error => {
    options.onDisconnected?.(error)
  })
}
