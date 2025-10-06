namespace Apollo.Discord.Services;

public class DailyAlertSetupSession
{
    public ulong? ChannelId { get; set; }
    public ulong? RoleId { get; set; }
    public string? Time { get; set; }
    public string? Message { get; set; }
}
