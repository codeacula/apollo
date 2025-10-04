# Settings Migration Guide: SettingKeys to IOptions

This guide explains how to migrate from using the deprecated `SettingKeys` constants to the new strongly-typed `IOptions<RydiaSettings>` pattern.

## Overview

The codebase has migrated from using static `SettingKeys` constants to a strongly-typed configuration approach using the `IOptions` pattern. This provides:

- **Compile-time safety**: TypeScript-like IntelliSense for settings
- **Better testability**: Easy to mock and inject test values
- **Validation support**: Built-in .NET configuration validation
- **Cleaner code**: No string literals scattered throughout the codebase

## What Changed

### Old Approach (Deprecated)
```csharp
using Rydia.Core.Constants;
using Rydia.Core.Services;

public class MyService
{
    private readonly ISettingsService _settingsService;
    
    public MyService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }
    
    public async Task DoSomething()
    {
        var channelId = await _settingsService.GetSettingAsync(SettingKeys.DailyAlertChannelId);
        var roleId = await _settingsService.GetSettingAsync(SettingKeys.DailyAlertRoleId);
        
        // Parse manually...
        if (ulong.TryParse(channelId, out var channel) && 
            ulong.TryParse(roleId, out var role))
        {
            // Use values...
        }
    }
}
```

### New Approach (Recommended)
```csharp
using Microsoft.Extensions.Options;
using Rydia.Core.Configuration;
using Rydia.Core.Services;

public class MyService
{
    private readonly RydiaSettings _settings;
    private readonly ISettingsProvider _settingsProvider;
    
    public MyService(IOptions<RydiaSettings> settings, ISettingsProvider settingsProvider)
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
using Rydia.Core.Configuration;
using Rydia.Core.Services;
```

Remove:
```csharp
using Rydia.Core.Constants; // Deprecated
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
public MyClass(IOptions<RydiaSettings> settings, ISettingsProvider settingsProvider)
{
    _settings = settings.Value;
    _settingsProvider = settingsProvider;
}
```

### 3. Update Settings Access

**Before:**
```csharp
var channelId = await _settingsService.GetSettingAsync(SettingKeys.DailyAlertChannelId);
var isDebug = await _settingsService.GetBooleanSettingAsync(SettingKeys.DebugLoggingEnabled);
```

**After:**
```csharp
var channelId = _settings.DailyAlertChannelId; // Already parsed as ulong?
var isDebug = _settings.DebugLoggingEnabled;    // Already parsed as bool
```

### 4. Update Settings Writes

When writing settings, you should reload the provider to update the cached values:

**Before:**
```csharp
await _settingsService.SetSettingAsync(SettingKeys.DailyAlertChannelId, "12345");
```

**After:**
```csharp
await _settingsService.SetSettingAsync("daily_alert_channel_id", "12345");
await _settingsProvider.ReloadAsync(); // Refresh the IOptions cache
```

## Available Settings

The `RydiaSettings` class includes:

```csharp
public class RydiaSettings
{
    public ulong? DailyAlertChannelId { get; set; }
    public ulong? DailyAlertRoleId { get; set; }
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
        var settings = new RydiaSettings 
        { 
            BotPrefix = "!",
            DebugLoggingEnabled = true 
        };
        var options = new TestOptions<RydiaSettings>(settings);
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

## API Usage Example

The `ApiController` demonstrates using IOptions to expose settings:

```csharp
[ApiController]
[Route("/api")]
public class ApiController : ControllerBase
{
    private readonly RydiaSettings _settings;

    public ApiController(IOptions<RydiaSettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("settings")]
    public ActionResult<RydiaSettings> GetSettings()
    {
        return Ok(_settings);
    }
}
```

## Backwards Compatibility

The `SettingKeys` class is marked as `[Obsolete]` but still functional during the transition period. The database schema and `ISettingsService` remain unchanged for backwards compatibility.

To suppress obsolete warnings during migration, add to your `.csproj`:
```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);CS0618</NoWarn>
</PropertyGroup>
```

## Benefits

1. **Type Safety**: No more manual string parsing
2. **Performance**: Settings cached in memory, no database queries on each access
3. **IntelliSense**: Full IDE support with property discovery
4. **Testability**: Easy to mock and inject test values
5. **Validation**: Built-in support for .NET configuration validation

## Questions?

For questions about the migration, please open an issue in the GitHub repository.
