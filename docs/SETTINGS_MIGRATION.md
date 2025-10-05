# Settings Migration Guide: IOptions Pattern

This guide explains the strongly-typed `IOptions<ApolloSettings>` pattern for configuration management.

## Overview

The codebase uses a strongly-typed configuration approach using the `IOptions` pattern with database-backed settings. This provides:

- **Compile-time safety**: TypeScript-like IntelliSense for settings
- **Better testability**: Easy to mock and inject test values
- **Validation support**: Built-in .NET configuration validation
- **No magic strings**: All database keys are constants in `ApolloSettings.Keys`

## What Changed

## Usage Pattern
```csharp
using Microsoft.Extensions.Options;
using Apollo.Core.Configuration;
using Apollo.Core.Services;

public class MyService
{
    private readonly ApolloSettings _settings;
    private readonly ISettingsProvider _settingsProvider;
    
    public MyService(IOptions<ApolloSettings> settings, ISettingsProvider settingsProvider)
    {
        _settings = settings.Value;
        _settingsProvider = settingsProvider;
    }
    
    public void DoSomething()
    {
        // No async, no parsing - just use!
        var channelId = _settings.DailyAlertChannelId;
        var roleId = _settings.DailyAlertRoleId;
        
        if (channelId.HasValue && roleId.HasValue)
        {
            // Use values directly - they're already parsed!
        }
    }
    
    public async Task UpdateAndReload()
    {
        // If you need to update settings and reload immediately:
        await _settingsProvider.ReloadAsync();
        
        // Now _settings.Value will have the updated values
    }
}
```

## Migration Steps

### 1. Update Dependencies

Add these using statements:
```csharp
using Microsoft.Extensions.Options;
using Apollo.Core.Configuration;
using Apollo.Core.Services;
```

### 2. Update Constructor Injection

**Before:**
```csharp
public MyClass(ISettingsService settingsService)
{
    _settingsService = settingsService;
}
```

**After:**
```csharp
public MyClass(IOptions<ApolloSettings> settings, ISettingsProvider settingsProvider)
{
    _settings = settings.Value;
    _settingsProvider = settingsProvider;
}
```

### 3. Update Settings Access

Access settings through the strongly-typed properties:
```csharp
var channelId = _settings.DailyAlertChannelId; // Already parsed as ulong?
var isDebug = _settings.DebugLoggingEnabled;    // Already parsed as bool
```

### 4. Update Settings Writes

When writing settings, use the constants from `ApolloSettings.Keys` and reload the provider:

```csharp
await _settingsService.SetSettingAsync(ApolloSettings.Keys.DailyAlertChannelId, "12345");
await _settingsProvider.ReloadAsync(); // Refresh the IOptions cache
```

## Available Settings

The `ApolloSettings` class includes database key constants and strongly-typed properties:

```csharp
public class ApolloSettings
{
    // Database key constants
    public static class Keys
    {
        public const string DailyAlertChannelId = "daily_alert_channel_id";
        public const string DailyAlertRoleId = "daily_alert_role_id";
        public const string DailyAlertTime = "daily_alert_time";
        public const string DailyAlertInitialMessage = "daily_alert_initial_message";
        public const string DefaultTimezone = "default_timezone";
        public const string BotPrefix = "bot_prefix";
        public const string DebugLoggingEnabled = "debug_logging_enabled";
    }

    // Strongly-typed properties
    public ulong? DailyAlertChannelId { get; set; }
    public ulong? DailyAlertRoleId { get; set; }
    public string? DailyAlertTime { get; set; }
    public string? DailyAlertInitialMessage { get; set; }
    public string? DefaultTimezone { get; set; }
    public string? BotPrefix { get; set; }
    public bool DebugLoggingEnabled { get; set; }
}
```

## Testing

When writing tests, create a simple `IOptions<T>` implementation:

```csharp
public class MyServiceTests
{
    [Fact]
    public void TestMethod()
    {
        // Arrange
        var settings = new ApolloSettings 
        { 
            BotPrefix = "!",
            DebugLoggingEnabled = true 
        };
        var options = new TestOptions<ApolloSettings>(settings);
        var service = new MyService(options);
        
        // Act & Assert...
    }
    
    private class TestOptions<T> : IOptions<T> where T : class
    {
        public TestOptions(T value) => Value = value;
        public T Value { get; }
    }
}
```





## Benefits

1. **Type Safety**: No more manual string parsing
2. **Performance**: Settings cached in memory, no database queries on each access
3. **IntelliSense**: Full IDE support with property discovery
4. **Testability**: Easy to mock and inject test values
5. **No Magic Strings**: All database keys are constants in `ApolloSettings.Keys`
6. **Validation**: Built-in support for .NET configuration validation
