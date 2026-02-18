# Apollo Is Too Picky About The Time Format

## Description

Apollo's time parsing is overly restrictive. The `FuzzyTimeParser` only supports 4 narrow patterns (`in N unit`, `tomorrow`, `next week`), and the fallback to `DateTime.TryParse` is unreliable (no explicit culture or format specifiers). There is also a mismatch between what the AI prompts tell the LLM to produce (e.g., `"tomorrow at 3pm"`) and what the parser can actually accept. Additionally, the parsing logic is duplicated across three locations (`ToDoPlugin`, `RemindersPlugin`, `ApolloGrpcService`) with subtle differences (the gRPC version doesn't do timezone-aware conversion).

**Goal:** Expand the accepted time formats by (1) adding more regex patterns to `FuzzyTimeParser`, (2) improving the C# `DateTime.TryParse` fallback with explicit format strings, (3) consolidating all parsing into a shared service, and (4) as a last resort, creating an efficient LLM prompt specifically for time parsing. Fix the prompt/parser mismatch so the LLM knows what formats are actually accepted.

**GitHub Issue:** #180

## Steps

### Step 1 - Expand FuzzyTimeParser With New Patterns

Add new source-generated regex patterns to support common natural language time expressions that users expect to work. This step is self-contained and affects only the parser and its tests.

**New patterns to support:**
- `tomorrow at 3pm` / `tomorrow at 15:00` (compound day + time-of-day)
- `at 3pm` / `at 15:00` / `at noon` / `at midnight` (time-of-day today)
- `tonight` / `this morning` / `this afternoon` / `this evening` (time-of-day aliases)
- `noon` / `midnight` (standalone time aliases)
- `next Monday` / `on Tuesday` / `on Friday at 3pm` (day-of-week with optional time)
- `in an hour` / `in half an hour` (word-based durations)
- `5 minutes` / `30 minutes` (without "in" prefix — lenient matching)
- `end of day` / `end of week` (common natural anchors)

#### Files

#### `src/Apollo.Application/ToDos/FuzzyTimeParser.cs`: Add new `[GeneratedRegex]` patterns and corresponding parse branches

The current parser has only 3 regex patterns (`InRelativeTimeRegex`, `TomorrowRegex`, `NextWeekRegex`). Add new patterns for each supported expression type. Each new pattern should follow the existing convention of using `[GeneratedRegex]` source generators and `RegexOptions.IgnoreCase | RegexOptions.Compiled`. Add corresponding match-and-compute logic in `TryParseFuzzyTime` following the same `Result<DateTime>` return pattern.

#### Tests

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: Add test methods for each new pattern

Add tests following the existing naming convention (e.g., `TryParseFuzzyTimeWithTomorrowAtTimeReturnsFutureDate`). Cover:
- `TryParseFuzzyTimeWithTomorrowAtTimeReturnsCorrectDateTime` — "tomorrow at 3pm", "tomorrow at 15:00"
- `TryParseFuzzyTimeWithAtTimeReturnsTodayAtTime` — "at 3pm", "at 15:00", "at noon", "at midnight"
- `TryParseFuzzyTimeWithTonightReturnsEveningTime` — "tonight"
- `TryParseFuzzyTimeWithThisMorningReturnsMorningTime` — "this morning"
- `TryParseFuzzyTimeWithThisAfternoonReturnsAfternoonTime` — "this afternoon"
- `TryParseFuzzyTimeWithThisEveningReturnsEveningTime` — "this evening"
- `TryParseFuzzyTimeWithNoonReturnsTodayAtNoon` — "noon"
- `TryParseFuzzyTimeWithMidnightReturnsTomorrowMidnight` — "midnight"
- `TryParseFuzzyTimeWithNextDayOfWeekReturnsCorrectDate` — "next Monday", "next Friday"
- `TryParseFuzzyTimeWithOnDayOfWeekReturnsCorrectDate` — "on Tuesday", "on Wednesday"
- `TryParseFuzzyTimeWithDayOfWeekAtTimeReturnsCorrectDateTime` — "on Friday at 3pm", "next Monday at 9am"
- `TryParseFuzzyTimeWithInAnHourReturnsOneHourFromNow` — "in an hour"
- `TryParseFuzzyTimeWithInHalfAnHourReturnsThirtyMinutesFromNow` — "in half an hour"
- `TryParseFuzzyTimeWithDurationWithoutPrefixReturnsFutureDate` — "5 minutes", "30 minutes", "2 hours"
- `TryParseFuzzyTimeWithEndOfDayReturnsEndOfDay` — "end of day", "eod"
- `TryParseFuzzyTimeWithEndOfWeekReturnsEndOfWeek` — "end of week"

---

### Step 2 - Create Consolidated Time Parsing Service

Extract the duplicated parse-then-fallback logic from `ToDoPlugin`, `RemindersPlugin`, and `ApolloGrpcService` into a single shared service. This service implements the full parsing pipeline: FuzzyTimeParser → C# TryParseExact with common formats → C# TryParse with InvariantCulture → (future) LLM fallback.

#### Files

#### `src/Apollo.Core/ToDos/ITimeParsingService.cs`: Create new interface

Define the interface for the consolidated time parsing service:
```csharp
public interface ITimeParsingService
{
  Task<Result<DateTime>> ParseTimeAsync(string input, string? userTimeZoneId = null, CancellationToken cancellationToken = default);
}
```
This interface abstracts all parsing strategies behind a single method. The `userTimeZoneId` parameter handles timezone-aware conversion (replacing the duplicated `ConvertToUtcAsync` methods). The return value is always UTC.

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Create new implementation

Implement the parsing pipeline:
1. Try `IFuzzyTimeParser.TryParseFuzzyTime()` — handles natural language patterns
2. Try `DateTime.TryParseExact()` with an array of common format strings (ISO 8601 variants, "h:mm tt", "hh:mm tt", "HH:mm", "MMM d", "MMMM d", etc.) using `CultureInfo.InvariantCulture`
3. Try `DateTime.TryParse()` with `CultureInfo.InvariantCulture` and `DateTimeStyles.None` as a general fallback
4. Return `Result.Fail()` with a helpful error message listing supported format examples

Include the timezone conversion logic currently duplicated in `ToDoPlugin.ConvertToUtcAsync` and `RemindersPlugin.ConvertToUtcAsync`. The service should look up the user's timezone from `IPersonStore` using the `userTimeZoneId` parameter and convert local times to UTC. Inject `IPersonStore`, `IFuzzyTimeParser`, and `TimeProvider`.

#### `src/Apollo.Application/ServiceCollectionExtension.cs`: Register ITimeParsingService

Add `services.AddScoped<ITimeParsingService, TimeParsingService>();` to the DI registration. Scoped because it uses `IPersonStore` which is scoped.

#### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: Create comprehensive tests

Test the full pipeline:
- `ParseTimeAsyncWithFuzzyTimeReturnsParsedUtcDateTime` — verifies FuzzyTimeParser is tried first
- `ParseTimeAsyncWithIso8601ReturnsUtcDateTime` — "2025-12-31T10:00:00"
- `ParseTimeAsyncWithCommonTimeFormatReturnsUtcDateTime` — "3:00 PM", "15:00"
- `ParseTimeAsyncWithDateTimeReturnsParsedDateTime` — "December 31, 2025"
- `ParseTimeAsyncWithInvalidInputReturnsFailure` — "not a time at all"
- `ParseTimeAsyncWithUserTimeZoneConvertsToUtc` — verify timezone conversion using mocked IPersonStore
- `ParseTimeAsyncWithoutUserTimeZoneAssumesUtc` — verify behavior when no timezone is provided
- `ParseTimeAsyncWithUnspecifiedKindTreatsAsUserLocal` — verify DateTimeKind.Unspecified is treated as user-local when timezone is known

---

### Step 3 - Create LLM Time Parsing Prompt (Last Resort Fallback)

Create an efficient, minimal prompt that asks the LLM to convert a natural language time expression to ISO 8601 format. This is the last-resort fallback when all other parsing strategies fail. The prompt should be designed for minimal token usage and fast response.

#### Files

#### `src/Apollo.AI/Prompts/ApolloTimeParsing.yml`: Create new prompt template

Create a minimal prompt that:
- Receives `current_datetime`, `user_timezone`, and the raw `time_expression` as template variables
- Instructs the LLM to return ONLY an ISO 8601 datetime string (no explanation)
- Includes a few-shot examples for clarity
- Is designed to be as short as possible for efficiency (small input/output)
- Specifies that if the expression cannot be parsed, return "UNPARSEABLE"

#### `src/Apollo.Core/ToDos/ITimeParsingService.cs`: No additional changes needed

The LLM fallback is an internal implementation detail of `TimeParsingService`. The interface remains the same. The service internally calls the AI agent when simpler methods fail.

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Add LLM fallback step

After step 3 (C# TryParse) fails, add step 4: call the LLM with the `ApolloTimeParsing` prompt. Parse the LLM's ISO 8601 response with `DateTime.TryParse`. If the LLM returns "UNPARSEABLE" or the response can't be parsed, return `Result.Fail()`. Inject `IApolloAIAgent` (or a lighter LLM client) for this.

#### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: Add LLM fallback tests

- `ParseTimeAsyncWhenAllParsersFallsBackToLlmReturnsUtcDateTime` — mock the AI agent to return a valid ISO 8601 string
- `ParseTimeAsyncWhenLlmReturnsUnparseableReturnsFailure` — mock the AI agent to return "UNPARSEABLE"
- `ParseTimeAsyncWhenLlmReturnsInvalidFormatReturnsFailure` — mock the AI agent to return garbage
- `ParseTimeAsyncDoesNotCallLlmWhenFuzzyParserSucceeds` — verify the LLM is NOT called when earlier stages succeed (efficiency)
- `ParseTimeAsyncDoesNotCallLlmWhenCSharpParseSucceeds` — verify the LLM is NOT called when C# parsing succeeds

---

### Step 4 - Refactor Consumers to Use TimeParsingService

Replace the duplicated parsing logic in `ToDoPlugin`, `RemindersPlugin`, and `ApolloGrpcService` with calls to the new `ITimeParsingService`. Remove the private `ParseReminderDateAsync`, `ParseReminderTimeAsync`, `ParseReminderTime`, and `ConvertToUtcAsync` methods.

#### Files

#### `src/Apollo.Application/ToDos/ToDoPlugin.cs`: Replace ParseReminderDateAsync and ConvertToUtcAsync with ITimeParsingService

Remove the private `ParseReminderDateAsync` method (lines 552-570) and `ConvertToUtcAsync` method (lines 572-591). Inject `ITimeParsingService` via the constructor. In `CreateToDoAsync`, replace the call to `ParseReminderDateAsync` with `await _timeParsingService.ParseTimeAsync(reminderDate, userTimeZoneId, cancellationToken)`. The `userTimeZoneId` should come from the person context already available in the plugin.

#### `src/Apollo.Application/Reminders/RemindersPlugin.cs`: Replace ParseReminderTimeAsync and ConvertToUtcAsync with ITimeParsingService

Remove the private `ParseReminderTimeAsync` method (lines 97-115) and `ConvertToUtcAsync` method (lines 117-136). Inject `ITimeParsingService` via the constructor. In `CreateReminderAsync`, replace the call to `ParseReminderTimeAsync` with `await _timeParsingService.ParseTimeAsync(reminderTime, userTimeZoneId, cancellationToken)`.

#### `src/Apollo.GRPC/Service/ApolloGrpcService.cs`: Replace ParseReminderTime with ITimeParsingService

Remove the private `ParseReminderTime` method (lines 105-131). Inject `ITimeParsingService` via the constructor. In `CreateReminderAsync`, replace the call to `ParseReminderTime` with `await _timeParsingService.ParseTimeAsync(reminderTime, null, cancellationToken)`. Note: the gRPC service currently doesn't do timezone-aware conversion, so pass `null` for `userTimeZoneId` to maintain backwards compatibility (or improve by passing the user's timezone if available from the request context).

#### Tests

#### `tests/Apollo.Application.Tests/ToDos/ToDoPluginTests.cs`: Update tests to verify ITimeParsingService usage

Update existing tests that cover `CreateToDoAsync` to mock `ITimeParsingService` instead of `IFuzzyTimeParser`. Verify that `ParseTimeAsync` is called with the correct arguments. Remove any tests that directly tested the now-removed private parsing methods (if any were tested indirectly).

#### `tests/Apollo.Application.Tests/Reminders/RemindersPluginTests.cs`: Update tests to verify ITimeParsingService usage

Update existing tests that cover `CreateReminderAsync` to mock `ITimeParsingService`. Verify delegation to the shared service.

#### `tests/Apollo.GRPC.Tests/Service/ApolloGrpcServiceTests.cs`: Update tests to verify ITimeParsingService usage

Update existing tests for `CreateReminderAsync` to mock `ITimeParsingService`. Verify delegation and that timezone conversion behavior is preserved.

---

### Step 5 - Update AI Prompt Files and Plugin Descriptions

Fix the mismatch between what the LLM is told it can produce and what the parser actually accepts. Now that the parser supports more formats, update the prompts and plugin `[Description]` attributes to accurately reflect the expanded capabilities.

#### Files

#### `src/Apollo.AI/Prompts/ApolloToolPlanning.yml`: Update time format examples

The existing examples already show `"tomorrow at 3pm"` (which will now be supported). Review and add more examples that demonstrate the newly supported formats, such as "tonight", "next Monday", "at 3pm", "in an hour". Ensure the examples accurately reflect what `TimeParsingService` can parse.

#### `src/Apollo.AI/Prompts/ApolloToolCalling.yml`: Update time-related instructions

The existing instructions reference natural language like "10 minutes from now", "on the 3rd". Update these to accurately list supported formats. Add guidance that the LLM should prefer simple, parseable formats (fuzzy time or ISO 8601) over complex natural language to avoid hitting the LLM fallback unnecessarily.

#### `src/Apollo.Application/ToDos/ToDoPlugin.cs`: Update [Description] attribute on reminderDate parameter

Change the `[Description]` from listing only `'in 10 minutes', 'in 2 hours', 'tomorrow', 'next week'` to include the newly supported formats. Example: `"Supports natural language like 'in 10 minutes', 'tomorrow at 3pm', 'next Monday', 'tonight', 'at noon', or ISO 8601 format."`.

#### `src/Apollo.Application/Reminders/RemindersPlugin.cs`: Update [Description] attribute on reminderTime parameter

Same change as ToDoPlugin — update the description to reflect all supported formats.

#### Tests

#### No new tests needed for this step

The prompt changes are configuration/documentation. The `[Description]` attribute changes affect LLM behavior but are not testable in unit tests. Integration testing of prompt quality is out of scope for this issue.
