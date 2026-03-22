export interface SubsystemStatus {
  ai: boolean
  discord: boolean
  superAdmin: boolean
}

export interface ConfigurationStatus {
  isConfigured: boolean
  subsystems: SubsystemStatus
}

export interface SetupStatusResponse {
  isInitialized: boolean
  isAiConfigured: boolean
  isDiscordConfigured: boolean
  isSuperAdminConfigured: boolean
}

export async function getSetupStatus(): Promise<SetupStatusResponse> {
  const response = await fetch('/api/setup/status')

  if (!response.ok) {
    throw new Error(`Failed to fetch configuration status: ${response.statusText}`)
  }

  return await response.json() as SetupStatusResponse
}

export async function getConfigurationStatus(): Promise<ConfigurationStatus> {
  const data = await getSetupStatus()

  return {
    isConfigured: data.isAiConfigured && data.isDiscordConfigured && data.isSuperAdminConfigured,
    subsystems: {
      ai: data.isAiConfigured,
      discord: data.isDiscordConfigured,
      superAdmin: data.isSuperAdminConfigured,
    },
  }
}
