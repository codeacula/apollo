interface SetupConfiguration {
  modelId: string
  endpoint: string
  apiKey: string
  token: string
  publicKey: string
  botName: string
  discordUserId: string
}

export async function submitSetupConfiguration(
  config: SetupConfiguration
): Promise<Response> {
  const response = await fetch('/api/setup', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      ai: {
        modelId: config.modelId,
        endpoint: config.endpoint,
        apiKey: config.apiKey,
      },
      discord: {
        token: config.token,
        publicKey: config.publicKey,
        botName: config.botName,
      },
      superAdmin: {
        discordUserId: config.discordUserId,
      },
    }),
  })

  if (!response.ok) {
    const error = await response.text()
    throw new Error(`Setup failed: ${response.statusText}. ${error}`)
  }

  return response
}
