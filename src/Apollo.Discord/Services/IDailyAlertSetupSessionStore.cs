namespace Apollo.Discord.Services;

public interface IDailyAlertSetupSessionStore
{
  Task<DailyAlertSetupSession?> GetSessionAsync(ulong guildId, ulong userId);
  Task SetSessionAsync(ulong guildId, ulong userId, DailyAlertSetupSession session);
  Task DeleteSessionAsync(ulong guildId, ulong userId);
}
