using Apollo.Core.People;
using Apollo.Database.People;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;

namespace Apollo.Database.Tests.Stores;

public sealed class PersonStoreTests(StoreTestFixture fixture) : IClassFixture<StoreTestFixture>
{
  private readonly StoreTestFixture _fixture = fixture;

  #region CreateByPlatformIdAsync Tests

  [Fact]
  public async Task CreateByPlatformIdAsyncWithValidPlatformIdReturnsSuccessAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.CreateByPlatformIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(platformId.Username, result.Value.Username.Value);
  }

  [Fact]
  public async Task CreateByPlatformIdAsyncPersistsPersonInDatabaseAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId("alice");
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var getResult = await store.GetByPlatformIdAsync(platformId);

    // Assert
    Assert.True(createResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(createResult.Value.Id, getResult.Value.Id);
  }

  [Fact]
  public async Task CreateByPlatformIdAsyncWithNullPlatformIdThrowsExceptionAsync()
  {
    // Arrange
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act & Assert
    _ = await Assert.ThrowsAsync<ArgumentException>(() =>
      store.CreateByPlatformIdAsync(default));
  }

  [Fact]
  public async Task CreateByPlatformIdAsyncGrantsAccessToSuperAdminAsync()
  {
    // Arrange
    const string superAdminId = "123456789";
    var superAdminConfig = new SuperAdminConfig { DiscordUserId = superAdminId };
    var platformId = new PlatformId("superadmin", superAdminId, Platform.Discord);
    var store = new PersonStore(superAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.CreateByPlatformIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Value.HasAccess.Value);
  }

  [Fact]
  public async Task CreateByPlatformIdAsyncDoesNotGrantAccessToNonSuperAdminAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.CreateByPlatformIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.False(result.Value.HasAccess.Value);
  }

  #endregion CreateByPlatformIdAsync Tests

  #region GetAsync Tests

  [Fact]
  public async Task GetAsyncWithValidIdReturnsPersonAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;

    // Act
    var result = await store.GetAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(personId, result.Value.Id);
  }

  [Fact]
  public async Task GetAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.GetAsync(invalidId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion GetAsync Tests

  #region GetByPlatformIdAsync Tests

  [Fact]
  public async Task GetByPlatformIdAsyncWithValidPlatformIdReturnsPersonAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId("bob");
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    _ = await store.CreateByPlatformIdAsync(platformId);

    // Act
    var result = await store.GetByPlatformIdAsync(platformId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal("bob", result.Value.Username.Value);
  }

  [Fact]
  public async Task GetByPlatformIdAsyncWithInvalidPlatformIdReturnsFailureAsync()
  {
    // Arrange
    var invalidPlatformId = StoreTestFixture.CreateTestPlatformId("nonexistent");
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.GetByPlatformIdAsync(invalidPlatformId);

    // Assert
    Assert.True(result.IsFailed);
  }

  [Fact]
  public async Task GetByPlatformIdAsyncFindsPeopleByDifferentPlatformsAsync()
  {
    // Arrange
    var discordId = new PlatformId("user1", "discord123", Platform.Discord);
    var twitchId = new PlatformId("user1", "twitch456", Platform.Twitch);
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    _ = await store.CreateByPlatformIdAsync(discordId);
    _ = await store.CreateByPlatformIdAsync(twitchId);

    // Act
    var discordResult = await store.GetByPlatformIdAsync(discordId);
    var twitchResult = await store.GetByPlatformIdAsync(twitchId);

    // Assert
    Assert.True(discordResult.IsSuccess);
    Assert.True(twitchResult.IsSuccess);
    Assert.NotEqual(discordResult.Value.Id, twitchResult.Value.Id);
  }

  #endregion GetByPlatformIdAsync Tests

  #region GrantAccessAsync Tests

  [Fact]
  public async Task GrantAccessAsyncUpdatesPersonAccessAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;
    Assert.False(createResult.Value.HasAccess.Value);

    // Act
    var grantResult = await store.GrantAccessAsync(personId);
    var getResult = await store.GetAsync(personId);

    // Assert
    Assert.True(grantResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.True(getResult.Value.HasAccess.Value);
  }

  [Fact]
  public async Task GrantAccessAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.GrantAccessAsync(invalidId);

    // Assert
    // Note: This test documents current behavior - may need adjustment based on actual implementation
    Assert.False(result.IsSuccess);
  }

  #endregion GrantAccessAsync Tests

  #region RevokeAccessAsync Tests

  [Fact]
  public async Task RevokeAccessAsyncRemovesPersonAccessAsync()
  {
    // Arrange
    const string superAdminId = "revoke_test_admin";
    var superAdminConfig = new SuperAdminConfig { DiscordUserId = superAdminId };
    var platformId = new PlatformId("admin", superAdminId, Platform.Discord);
    var store = new PersonStore(superAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;
    Assert.True(createResult.Value.HasAccess.Value);

    // Act
    var revokeResult = await store.RevokeAccessAsync(personId);
    var getResult = await store.GetAsync(personId);

    // Assert
    Assert.True(revokeResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.False(getResult.Value.HasAccess.Value);
  }

  [Fact]
  public async Task RevokeAccessAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.RevokeAccessAsync(invalidId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion RevokeAccessAsync Tests

  #region GetAccessAsync Tests

  [Fact]
  public async Task GetAccessAsyncWithValidIdReturnsAccessStatusAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;

    // Act
    var result = await store.GetAccessAsync(personId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Equal(createResult.Value.HasAccess.Value, result.Value.Value);
  }

  [Fact]
  public async Task GetAccessAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.GetAccessAsync(invalidId);

    // Assert
    Assert.True(result.IsFailed);
  }

  #endregion GetAccessAsync Tests

  #region SetTimeZoneAsync Tests

  [Fact]
  public async Task SetTimeZoneAsyncUpdatesPersonTimeZoneAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;
    var newTimeZoneId = StoreTestFixture.CreateTestPersonTimeZoneId();

    // Act
    var updateResult = await store.SetTimeZoneAsync(personId, newTimeZoneId);
    var getResult = await store.GetAsync(personId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(newTimeZoneId.Value, getResult.Value.TimeZoneId?.Value);
  }

  [Fact]
  public async Task SetTimeZoneAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var timeZoneId = StoreTestFixture.CreateTestPersonTimeZoneId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.SetTimeZoneAsync(invalidId, timeZoneId);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion SetTimeZoneAsync Tests

  #region SetDailyTaskCountAsync Tests

  [Fact]
  public async Task SetDailyTaskCountAsyncUpdatesPersonDailyTaskCountAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var personId = createResult.Value.Id;
    var dailyTaskCount = new DailyTaskCount(10);

    // Act
    var updateResult = await store.SetDailyTaskCountAsync(personId, dailyTaskCount);
    var getResult = await store.GetAsync(personId);

    // Assert
    Assert.True(updateResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.Equal(10, getResult.Value.DailyTaskCount?.Value);
  }

  [Fact]
  public async Task SetDailyTaskCountAsyncWithInvalidIdReturnsFailureAsync()
  {
    // Arrange
    var invalidId = new PersonId(Guid.NewGuid());
    var dailyTaskCount = new DailyTaskCount(5);
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);

    // Act
    var result = await store.SetDailyTaskCountAsync(invalidId, dailyTaskCount);

    // Assert
    Assert.False(result.IsSuccess);
  }

  #endregion SetDailyTaskCountAsync Tests

  #region NotificationChannel Tests

  [Fact]
  public async Task AddNotificationChannelAsyncAddsChannelToPersonAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var person = createResult.Value;
    var channel = new NotificationChannel(NotificationChannelType.Discord, person.PlatformId.PlatformUserId, true);

    // Act
    var addResult = await store.AddNotificationChannelAsync(person, channel);
    var getResult = await store.GetAsync(person.Id);

    // Assert
    Assert.True(addResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.NotEmpty(getResult.Value.NotificationChannels);
  }

  [Fact]
  public async Task RemoveNotificationChannelAsyncRemovesChannelFromPersonAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var person = createResult.Value;
    var channel = new NotificationChannel(NotificationChannelType.Discord, person.PlatformId.PlatformUserId, true);
    _ = await store.AddNotificationChannelAsync(person, channel);

    // Act
    var removeResult = await store.RemoveNotificationChannelAsync(person, channel);

    // Assert
    Assert.True(removeResult.IsSuccess);
  }

  [Fact]
  public async Task ToggleNotificationChannelAsyncTogglesChannelStateAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var person = createResult.Value;
    var channel = new NotificationChannel(NotificationChannelType.Discord, person.PlatformId.PlatformUserId, true);
    _ = await store.AddNotificationChannelAsync(person, channel);

    // Act
    var toggleResult = await store.ToggleNotificationChannelAsync(person, channel);
    var getResult = await store.GetAsync(person.Id);

    // Assert
    Assert.True(toggleResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
  }

  [Fact]
  public async Task EnsureNotificationChannelAsyncAddsChannelIfNotExistsAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var person = createResult.Value;
    var channel = new NotificationChannel(NotificationChannelType.Discord, person.PlatformId.PlatformUserId, true);

    // Act
    var ensureResult = await store.EnsureNotificationChannelAsync(person, channel);
    var getResult = await store.GetAsync(person.Id);

    // Assert
    Assert.True(ensureResult.IsSuccess);
    Assert.True(getResult.IsSuccess);
    Assert.NotEmpty(getResult.Value.NotificationChannels);
  }

  [Fact]
  public async Task EnsureNotificationChannelAsyncReplacesExistingChannelWithDifferentIdentifierAsync()
  {
    // Arrange
    var platformId = StoreTestFixture.CreateTestPlatformId();
    var store = new PersonStore(_fixture.SuperAdminConfig, _fixture.DocumentSession, _fixture.TimeProvider, _fixture.PersonCache);
    var createResult = await store.CreateByPlatformIdAsync(platformId);
    var person = createResult.Value;
    var oldChannel = new NotificationChannel(NotificationChannelType.Discord, "old_identifier", true);
    var newChannel = new NotificationChannel(NotificationChannelType.Discord, "new_identifier", true);
    _ = await store.AddNotificationChannelAsync(person, oldChannel);

    // Act
    var ensureResult = await store.EnsureNotificationChannelAsync(person, newChannel);

    // Assert
    Assert.True(ensureResult.IsSuccess);
  }

  #endregion NotificationChannel Tests
}
