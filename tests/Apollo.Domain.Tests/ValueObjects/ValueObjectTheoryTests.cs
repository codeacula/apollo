using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.ValueObjects;

namespace Apollo.Domain.Tests.ValueObjects;

public sealed class ValueObjectTheoryTests
{
  public static TheoryData<string, string, string> StringValueObjectCases => new()
  {
    { nameof(Content), "Test content", "Other content" },
    { nameof(DisplayName), "Test Display Name", "Other display name" },
    { nameof(Description), "Buy groceries", "Do laundry" },
    { nameof(Details), "Need to get milk, eggs, and bread", "Pack laptop charger" }
  };

  public static TheoryData<string, string, string> DateTimeValueObjectCases => new()
  {
    { nameof(CreatedOn), "2026-01-01T10:00:00Z", "2026-01-01T10:01:00Z" },
    { nameof(UpdatedOn), "2026-01-01T10:02:00Z", "2026-01-01T10:03:00Z" },
    { nameof(DueDate), "2026-01-08T10:00:00Z", "2026-01-09T10:00:00Z" },
    { nameof(AcknowledgedOn), "2026-01-01T10:04:00Z", "2026-01-01T10:09:00Z" },
    { nameof(GrantedOn), "2026-01-01T10:05:00Z", "2026-01-01T10:06:00Z" }
  };

  public static TheoryData<string, string, string> GuidValueObjectCases => new()
  {
    { nameof(ConversationId), "d4c7b0bd-9ce5-4b75-b3d8-2e1e1bc43210", "4bf2db2d-5d4b-4d9d-8e16-a10ec3fa5b4a" },
    { nameof(MessageId), "4f2194ba-f24e-4a91-8e50-3cb3ccf857bb", "56290f77-37f7-4ea6-8b31-8046298ea39e" },
    { nameof(PersonId), "2e14a85e-cd1f-4fa2-a52f-b4cc5d4fb3cd", "d7b7407f-5f61-45e4-9d10-cb5b0de7f55f" },
    { nameof(ReminderId), "fd565214-e09f-4af0-bd29-57e7b52831ea", "f95e5b31-a296-4767-95ae-f1f6103f6c91" },
    { nameof(ToDoId), "de6e2697-65d4-4857-aef0-60aef7a6f3e2", "2192ec77-68ca-48de-b8af-fcd0ca2a7f4e" }
  };

  public static TheoryData<string, bool, bool> BooleanValueObjectCases => new()
  {
    { nameof(FromUser), true, false }
  };

  public static TheoryData<string, Level, Level> LevelValueObjectCases => new()
  {
    { nameof(Priority), Level.Red, Level.Blue },
    { nameof(Energy), Level.Yellow, Level.Blue },
    { nameof(Interest), Level.Green, Level.Blue }
  };

  [Theory]
  [MemberData(nameof(StringValueObjectCases))]
  public void StringValueObjectsStoreValueAndSupportEquality(string typeName, string value, string otherValue)
  {
    AssertValueObject(CreateStringValueObject(typeName), value, otherValue);
  }

  [Theory]
  [MemberData(nameof(DateTimeValueObjectCases))]
  public void DateTimeValueObjectsStoreValueAndSupportEquality(string typeName, string value, string otherValue)
  {
    AssertValueObject(
      CreateDateTimeValueObject(typeName),
      DateTime.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind),
      DateTime.Parse(otherValue, null, System.Globalization.DateTimeStyles.RoundtripKind));
  }

  [Theory]
  [MemberData(nameof(GuidValueObjectCases))]
  public void GuidValueObjectsStoreValueAndSupportEquality(string typeName, string value, string otherValue)
  {
    AssertValueObject(CreateGuidValueObject(typeName), Guid.Parse(value), Guid.Parse(otherValue));
  }

  [Theory]
  [MemberData(nameof(BooleanValueObjectCases))]
  public void BooleanValueObjectsStoreValueAndSupportEquality(string typeName, bool value, bool otherValue)
  {
    AssertValueObject(CreateBooleanValueObject(typeName), value, otherValue);
  }

  [Theory]
  [MemberData(nameof(LevelValueObjectCases))]
  public void LevelValueObjectsStoreValueAndSupportEquality(string typeName, Level value, Level otherValue)
  {
    AssertValueObject(CreateLevelValueObject(typeName), value, otherValue);
  }

  private static Func<string, object> CreateStringValueObject(string typeName) => typeName switch
  {
    nameof(Content) => value => new Content(value),
    nameof(DisplayName) => value => new DisplayName(value),
    nameof(Description) => value => new Description(value),
    nameof(Details) => value => new Details(value),
    _ => throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null)
  };

  private static Func<DateTime, object> CreateDateTimeValueObject(string typeName) => typeName switch
  {
    nameof(CreatedOn) => value => new CreatedOn(value),
    nameof(UpdatedOn) => value => new UpdatedOn(value),
    nameof(DueDate) => value => new DueDate(value),
    nameof(AcknowledgedOn) => value => new AcknowledgedOn(value),
    nameof(GrantedOn) => value => new GrantedOn(value),
    _ => throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null)
  };

  private static Func<Guid, object> CreateGuidValueObject(string typeName) => typeName switch
  {
    nameof(ConversationId) => value => new ConversationId(value),
    nameof(MessageId) => value => new MessageId(value),
    nameof(PersonId) => value => new PersonId(value),
    nameof(ReminderId) => value => new ReminderId(value),
    nameof(ToDoId) => value => new ToDoId(value),
    _ => throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null)
  };

  private static Func<bool, object> CreateBooleanValueObject(string typeName) => typeName switch
  {
    nameof(FromUser) => value => new FromUser(value),
    _ => throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null)
  };

  private static Func<Level, object> CreateLevelValueObject(string typeName) => typeName switch
  {
    nameof(Priority) => value => new Priority(value),
    nameof(Energy) => value => new Energy(value),
    nameof(Interest) => value => new Interest(value),
    _ => throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null)
  };

  private static void AssertValueObject<TValue>(Func<TValue, object> factory, TValue value, TValue otherValue)
  {
    var instance = factory(value);
    var sameValueInstance = factory(value);
    var differentValueInstance = factory(otherValue);
    var storedValue = instance.GetType().GetProperty("Value")!.GetValue(instance);

    Assert.Equal(value, storedValue);
    Assert.Equal(instance, sameValueInstance);
    Assert.NotEqual(instance, differentValueInstance);
  }
}
