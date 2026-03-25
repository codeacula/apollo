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
  id: number
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

const FETCH_TIMEOUT_MS = 10_000

async function extractErrorDetail(response: Response): Promise<string> {
  try {
    const contentType = response.headers.get('content-type') ?? ''

    if (contentType.includes('application/json')) {
      const body = await response.json() as unknown
      if (body && typeof body === 'object' && 'error' in body) {
        const detail = (body as { error?: unknown }).error
        return typeof detail === 'string' ? detail : JSON.stringify(detail)
      }
      return JSON.stringify(body)
    }

    return await response.text()
  } catch {
    return ''
  }
}

export async function getDashboardOverview(): Promise<DashboardOverview> {
  const controller = new AbortController()
  const timeoutId = setTimeout(() => controller.abort(), FETCH_TIMEOUT_MS)

  let response: Response

  try {
    response = await fetch('/api/dashboard/overview', { signal: controller.signal })
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError') {
      throw new Error('Dashboard overview request timed out')
    }
    throw err
  } finally {
    clearTimeout(timeoutId)
  }

  if (!response.ok) {
    const detail = await extractErrorDetail(response)
    const suffix = detail ? ` - ${detail}` : ''
    throw new Error(`Failed to fetch dashboard overview: ${response.status} ${response.statusText}${suffix}`)
  }

  return await response.json() as DashboardOverview
}
