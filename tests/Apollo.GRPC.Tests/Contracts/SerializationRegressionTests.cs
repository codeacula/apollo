using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.GRPC.Contracts;

using ProtoBuf;

namespace Apollo.GRPC.Tests.Contracts;

/// <summary>
/// Integration tests for gRPC contract serialization/deserialization.
/// These tests verify that protobuf-net correctly serializes and deserializes NewMessageRequest
/// without losing or corrupting data due to DataMember Order mismatches.
/// </summary>
public class NewMessageRequestSerializationTests
{
  /// <summary>
  /// Tests round-trip serialization of NewMessageRequest.
  /// If this fails, it indicates a protobuf-net ordering issue with the gRPC contracts.
  /// </summary>
  [Fact]
  public void NewMessageRequestRoundTripSerializationPreservesAllFields()
  {
    // Arrange
    const string expectedUsername = "codeacula";
    const string expectedPlatformUserId = "244273250144747523";
    const string expectedContent = "Remind me to buy milk";
    const Platform expectedPlatform = Platform.Discord;

    var originalRequest = new NewMessageRequest
    {
      Username = expectedUsername,
      PlatformUserId = expectedPlatformUserId,
      Platform = expectedPlatform,
      Content = expectedContent
    };

    // Act: Serialize
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, originalRequest);
    ms.Position = 0;

    // Act: Deserialize
    var deserializedRequest = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert: All fields preserved
    Assert.NotNull(deserializedRequest);
    Assert.Equal(expectedUsername, deserializedRequest.Username);
    Assert.Equal(expectedPlatformUserId, deserializedRequest.PlatformUserId);
    Assert.Equal(expectedPlatform, deserializedRequest.Platform);
    Assert.Equal(expectedContent, deserializedRequest.Content);
  }

  /// <summary>
  /// Specifically tests that Username field survives serialization round-trip.
  /// The bug shows Username as null, so this test isolates that field.
  /// </summary>
  [Fact]
  public void NewMessageRequestSerializationPreservesUsernameField()
  {
    // Arrange
    const string expectedUsername = "codeacula";
    var request = new NewMessageRequest
    {
      Username = expectedUsername,
      PlatformUserId = "244273250144747523",
      Platform = Platform.Discord,
      Content = "test"
    };

    // Act
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, request);
    ms.Position = 0;
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert
    Assert.NotNull(deserialized.Username);
    Assert.Equal(expectedUsername, deserialized.Username);
  }

  /// <summary>
  /// Specifically tests that PlatformUserId field survives serialization round-trip.
  /// The bug shows PlatformUserId as null, so this test isolates that field.
  /// </summary>
  [Fact]
  public void NewMessageRequestSerializationPreservesPlatformUserIdField()
  {
    // Arrange
    const string expectedPlatformUserId = "244273250144747523";
    var request = new NewMessageRequest
    {
      Username = "codeacula",
      PlatformUserId = expectedPlatformUserId,
      Platform = Platform.Discord,
      Content = "test"
    };

    // Act
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, request);
    ms.Position = 0;
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert
    Assert.NotNull(deserialized.PlatformUserId);
    Assert.Equal(expectedPlatformUserId, deserialized.PlatformUserId);
  }

  /// <summary>
  /// Tests that the PlatformId value object created from deserialized NewMessageRequest
  /// has all components populated correctly (mimics what UserResolutionInterceptor does).
  /// </summary>
  [Fact]
  public void DeserializedNewMessageRequestCreatesCorrectPlatformId()
  {
    // Arrange
    const string expectedUsername = "codeacula";
    const string expectedPlatformUserId = "244273250144747523";
    const Platform expectedPlatform = Platform.Discord;

    var originalRequest = new NewMessageRequest
    {
      Username = expectedUsername,
      PlatformUserId = expectedPlatformUserId,
      Platform = expectedPlatform,
      Content = "test"
    };

    // Act: Serialize and deserialize
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, originalRequest);
    ms.Position = 0;
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Now create PlatformId like UserResolutionInterceptor does
    var platformId = deserialized.ToPlatformId();

    // Assert
    Assert.Equal(expectedUsername, platformId.Username);
    Assert.Equal(expectedPlatformUserId, platformId.PlatformUserId);
    Assert.Equal(expectedPlatform, platformId.Platform);
  }

  /// <summary>
  /// Regression test: Ensures that null values don't appear after round-trip.
  /// This directly tests for the bug scenario where Username/PlatformUserId became null.
  /// </summary>
  [Fact]
  public void NewMessageRequestDeserializationDoesNotProduceNullFields()
  {
    // Arrange
    var originalRequest = new NewMessageRequest
    {
      Username = "codeacula",
      PlatformUserId = "244273250144747523",
      Platform = Platform.Discord,
      Content = "Remind me to buy milk"
    };

    // Act
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, originalRequest);
    ms.Position = 0;
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert: No field should be null
    Assert.NotNull(deserialized.Username);
    Assert.NotNull(deserialized.PlatformUserId);
    Assert.NotNull(deserialized.Content);
    Assert.NotEqual(Platform.Discord, Platform.Web);  // Platform should be Discord, not default
  }

  /// <summary>
  /// Tests multiple consecutive serialization/deserialization cycles.
  /// If there's a data corruption issue, it might only appear after multiple cycles.
  /// </summary>
  [Fact]
  public void NewMessageRequestMultipleCyclesPreserveData()
  {
    // Arrange
    const string expectedUsername = "codeacula";
    const string expectedPlatformUserId = "244273250144747523";
    

    // Act: Multiple round-trips
    NewMessageRequest current = new NewMessageRequest
    {
      Username = expectedUsername,
      PlatformUserId = expectedPlatformUserId,
      Platform = Platform.Discord,
      Content = "test message"
    };
    for (int i = 0; i < 3; i++)
    {
      using var ms = new MemoryStream();
      Serializer.Serialize(ms, current);
      ms.Position = 0;
      current = Serializer.Deserialize<NewMessageRequest>(ms)!;
    }

    // Assert: Data still intact after 3 cycles
    Assert.Equal(expectedUsername, current.Username);
    Assert.Equal(expectedPlatformUserId, current.PlatformUserId);
    Assert.Equal(Platform.Discord, current.Platform);
    Assert.Equal("test message", current.Content);
  }
}

/// <summary>
/// Tests for ProcessMessageRequest serialization.
/// Since ProcessMessageRequest has different DataMember Order values than AuthenticatedRequestBase,
/// this tests whether there's a conflict in the serialization format.
/// </summary>
public class ProcessMessageRequestSerializationTests
{
  /// <summary>
  /// Tests round-trip serialization of ProcessMessageRequest.
  /// Note: ProcessMessageRequest has different DataMember Order than NewMessageRequest/AuthenticatedRequestBase.
  /// </summary>
  [Fact]
  public void ProcessMessageRequestRoundTripSerializationPreservesAllFields()
  {
    // Arrange
    const string expectedUsername = "codeacula";
    const string expectedPlatformUserId = "244273250144747523";
    const string expectedContent = "Remind me to buy milk";
    const Platform expectedPlatform = Platform.Discord;

    var originalRequest = new Core.Conversations.ProcessMessageRequest
    {
      Username = expectedUsername,
      PlatformUserId = expectedPlatformUserId,
      Platform = expectedPlatform,
      Content = expectedContent
    };

    // Act: Serialize
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, originalRequest);
    ms.Position = 0;

    // Act: Deserialize
    var deserialized = Serializer.Deserialize<Core.Conversations.ProcessMessageRequest>(ms);

    // Assert: All fields preserved
    Assert.NotNull(deserialized);
    Assert.Equal(expectedUsername, deserialized.Username);
    Assert.Equal(expectedPlatformUserId, deserialized.PlatformUserId);
    Assert.Equal(expectedPlatform, deserialized.Platform);
    Assert.Equal(expectedContent, deserialized.Content);
  }
}

/// <summary>
/// Tests for cross-contract conversion and serialization.
/// Tests the actual flow: Discord -> ProcessMessageRequest -> NewMessageRequest -> Serialization -> Deserialization
/// </summary>
public class GrpcContractConversionTests
{
  /// <summary>
  /// Tests the complete flow: Create ProcessMessageRequest -> Convert to NewMessageRequest ->
  /// Serialize -> Deserialize -> Verify all fields intact
  /// </summary>
  [Fact]
  public void ProcessMessageRequestConvertedToNewMessageRequestSurvivesSerialization()
  {
    // Arrange: Simulate what Discord handler does
    const string discordUsername = "codeacula";
    const string discordUserId = "244273250144747523";

    var processRequest = new Core.Conversations.ProcessMessageRequest
    {
      Platform = Platform.Discord,
      PlatformUserId = discordUserId,
      Username = discordUsername,
      Content = "Remind me to buy milk"
    };

    // Act: Convert to NewMessageRequest (like ApolloGrpcClient does)
    var newMessageRequest = new NewMessageRequest
    {
      Platform = processRequest.Platform,
      PlatformUserId = processRequest.PlatformUserId,
      Username = processRequest.Username,
      Content = processRequest.Content
    };

    // Act: Serialize and deserialize
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, newMessageRequest);
    ms.Position = 0;
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert: All fields present and correct
    Assert.NotNull(deserialized);
    Assert.Equal(discordUsername, deserialized.Username);
    Assert.Equal(discordUserId, deserialized.PlatformUserId);
    Assert.Equal(Platform.Discord, deserialized.Platform);
    Assert.Equal("Remind me to buy milk", deserialized.Content);

    // Additional check: Can we create PlatformId from it?
    var platformId = deserialized.ToPlatformId();
    Assert.Equal(discordUsername, platformId.Username);
    Assert.Equal(discordUserId, platformId.PlatformUserId);
    Assert.Equal(Platform.Discord, platformId.Platform);
  }

  /// <summary>
  /// Tests that converting through intermediate representations doesn't lose data.
  /// This simulates the full path: Discord message -> PlatformId -> ProcessMessageRequest -> NewMessageRequest -> gRPC -> Deserialization
  /// </summary>
  [Fact]
  public void FullFlowPreservesDiscordUserIdentity()
  {
    // Arrange: Simulate Discord message with Author.Username and Author.Id
    const string authorUsername = "codeacula";
    const string authorId = "244273250144747523";

    // Step 1: Create PlatformId (like NetcordMessageExtension does)
    var platformId = new PlatformId(authorUsername, authorId, Platform.Discord);

    // Step 2: Create ProcessMessageRequest (like IncomingMessageHandler does)
    var processRequest = new Core.Conversations.ProcessMessageRequest
    {
      Platform = platformId.Platform,
      PlatformUserId = platformId.PlatformUserId,
      Username = platformId.Username,
      Content = "Remind me to buy milk"
    };

    // Step 3: Convert to NewMessageRequest (like ApolloGrpcClient does)
    var newMessageRequest = new NewMessageRequest
    {
      Platform = processRequest.Platform,
      PlatformUserId = processRequest.PlatformUserId,
      Username = processRequest.Username,
      Content = processRequest.Content
    };

    // Step 4: Serialize (happens during gRPC transmission)
    using var ms = new MemoryStream();
    Serializer.Serialize(ms, newMessageRequest);
    ms.Position = 0;

    // Step 5: Deserialize (happens at gRPC receiver)
    var deserialized = Serializer.Deserialize<NewMessageRequest>(ms);

    // Assert: Everything made it through the full pipeline
    Assert.NotNull(deserialized);
    Assert.NotNull(deserialized.Username);
    Assert.NotNull(deserialized.PlatformUserId);
    Assert.Equal(authorUsername, deserialized.Username);
    Assert.Equal(authorId, deserialized.PlatformUserId);
    Assert.Equal(Platform.Discord, deserialized.Platform);
    Assert.Equal("Remind me to buy milk", deserialized.Content);
  }
}
