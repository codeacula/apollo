export interface SubsystemStatus {
  ai: boolean
  discord: boolean
  superAdmin: boolean
}

export interface ConfigurationStatus {
  isConfigured: boolean
  subsystems: SubsystemStatus
}

export async function getConfigurationStatus(): Promise<ConfigurationStatus> {
  const response = await fetch('/api/configuration/status')

  if (!response.ok) {
    throw new Error(`Failed to fetch configuration status: ${response.statusText}`)
  }

  const data = await response.json()
  return data as ConfigurationStatus
}
