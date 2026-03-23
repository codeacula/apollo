export interface DashboardSubsystemStatus {
  ai: boolean
  discord: boolean
  superAdmin: boolean
}

export interface DashboardConfigurationStatus {
  isInitialized: boolean
  isConfigured: boolean
  subsystems: DashboardSubsystemStatus
}

export interface DashboardPeopleSummary {
  total: number
  withAccess: number
}

export interface DashboardToDoSummary {
  active: number
  completed: number
  createdToday: number
}

export interface DashboardReminderSummary {
  scheduled: number
  dueWithin24Hours: number
  sentToday: number
  acknowledged: number
}

export interface DashboardConversationSummary {
  total: number
  messagesLast24Hours: number
}

export interface DashboardActivityItem {
  kind: string
  title: string
  description: string
  occurredOnUtc: string
}

export interface DashboardOverview {
  generatedAtUtc: string
  configuration: DashboardConfigurationStatus
  people: DashboardPeopleSummary
  toDos: DashboardToDoSummary
  reminders: DashboardReminderSummary
  conversations: DashboardConversationSummary
  activity: DashboardActivityItem[]
}

export async function getDashboardOverview(): Promise<DashboardOverview> {
  const response = await fetch('/api/dashboard/overview')

  if (!response.ok) {
    throw new Error(`Failed to fetch dashboard overview: ${response.statusText}`)
  }

  return await response.json() as DashboardOverview
}
