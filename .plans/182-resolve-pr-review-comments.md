# Resolve PR #182 Review Comments

## Description

PR #182 ("✨ feat(reminders): expand time format flexibility and consolidate parsing") received 10 review
comments from GitHub Copilot. All comments identify real bugs or code quality issues in the new
`TimeParsingService`, `FuzzyTimeParser`, plugin whitespace validation, prompt accuracy, and timezone
handling. This plan addresses every comment with concrete code and test changes grouped into
parallelizable steps.

### Comment Summary

| # | File | Issue |
|---|------|-------|
| 1 | `ToDoPlugin.cs:42` | `IsNullOrEmpty` → `IsNullOrWhiteSpace` for `reminderDate` |
| 2 | `TimeParsingService.cs:153-154` | `FindSystemTimeZoneById` can throw; wrap in try/catch |
| 3 | `ApolloToolCalling.yml:45` | "tomorrow morning" listed but not deterministically parsed |
| 4 | `TimeParsingService.cs:72` | Fuzzy results skip `ConvertToUtc`; user-local times treated as UTC |
| 5 | `TimeParsingService.cs:27-29` | ISO formats with `Z`/offset use wrong `DateTimeStyles`; use `K` specifier |
| 6 | `ApolloToolPlanning.yml:93` | Same "tomorrow morning" issue as #3 |
| 7 | `RemindersPlugin.cs:35` | `IsNullOrEmpty` → `IsNullOrWhiteSpace` for `reminderTime` |
| 8 | `ApolloGrpcService.cs:73` | `ParseTimeAsync` called without user timezone |
| 9 | `TimeParsingService.cs:34-39` | Time-only formats produce DateTime with year 0001 |
| 10 | `FuzzyTimeParser.cs (TimeParserHelpers):333` | Bare hour 0–12 parsed as AM; restrict to 13–23 |

## Steps

### 1 - Fix whitespace validation in plugins (Comments 1, 7)

Both `ToDoPlugin.CreateToDoAsync` and `RemindersPlugin.CreateReminderAsync` use
`string.IsNullOrEmpty` to check for missing time input. Whitespace-only strings like `"  "` slip
through and hit the parsing service with a different error message. Change to `IsNullOrWhiteSpace`.

### Files

#### `src/Apollo.Application/ToDos/ToDoPlugin.cs`: Change `string.IsNullOrEmpty(reminderDate)` to `string.IsNullOrWhiteSpace(reminderDate)` on line 42
**Reason:** Whitespace-only `reminderDate` should be treated as "no reminder" and not forwarded to
the parsing service where it would fail with a different error message.

#### `src/Apollo.Application/Reminders/RemindersPlugin.cs`: Change `string.IsNullOrEmpty(reminderTime)` to `string.IsNullOrWhiteSpace(reminderTime)` on line 35
**Reason:** Same issue — whitespace-only `reminderTime` should be treated as missing input and
return the explicit "Reminder time is required" error.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/ToDoPluginTests.cs`: `CreateToDoAsyncWithWhitespaceReminderDateDoesNotCallTimeParsingServiceAsync`
**Reason:** Verify that a whitespace-only `reminderDate` is treated as no reminder and the parsing
service is never called.

#### `tests/Apollo.Application.Tests/Reminders/RemindersPluginTests.cs`: `CreateReminderAsyncWithWhitespaceTimeReturnsErrorAsync`
**Reason:** Verify that a whitespace-only `reminderTime` returns the "required" error immediately.

---

### 2 - Apply timezone conversion to fuzzy parser results (Comment 4)

`TimeParsingService.ParseTimeAsync` returns fuzzy parser results directly without calling
`ConvertToUtc`. For expressions like "tomorrow at 3pm" or "at 3pm", the parsed time is treated
as UTC rather than the user's local time. This causes reminders to be scheduled at the wrong time.

### Files

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Wrap fuzzy result with `ConvertToUtc(fuzzyResult.Value, userTimeZoneId)` at line 72
**Reason:** Fuzzy results for absolute clock times ("tomorrow at 3pm", "at noon", "tonight") need
the same timezone conversion as Steps 2-4. Change `return Result.Ok(fuzzyResult.Value)` to
`return Result.Ok(ConvertToUtc(fuzzyResult.Value, userTimeZoneId))`.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithFuzzyTimeAndUserTimezoneConvertsToUtcAsync`
**Reason:** Verify that when a fuzzy result is returned and a user timezone is provided, the result
is converted from user-local to UTC.

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithFuzzyTimeAndNoTimezoneReturnsUtcDirectlyAsync`
**Reason:** Verify that when no timezone is provided, fuzzy results are returned as-is (treated as
UTC).

---

### 3 - Fix ISO 8601 format specifiers for timezone-aware parsing (Comment 5)

The `ExactFormats` array uses literal `Z` and `zzz` format specifiers with `DateTimeStyles.None`,
which does not correctly interpret timezone offsets. Replace with `K` specifier and use
`DateTimeStyles.RoundtripKind` so `Z` is correctly parsed as UTC and offsets are preserved.

### Files

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Replace ISO format specifiers and add `RoundtripKind`
**Reason:** Replace `"yyyy-MM-ddTHH:mm:ssZ"` and `"yyyy-MM-ddTHH:mm:ss.fffffffZ"` with
`"yyyy-MM-ddTHH:mm:ssK"` and `"yyyy-MM-ddTHH:mm:ss.fffffffK"`. Remove the `"yyyy-MM-ddTHH:mm:sszzz"`
entry (now covered by `K`). Change `DateTimeStyles.None` to `DateTimeStyles.RoundtripKind` in the
`TryParseExact` call so that `Z` is correctly interpreted as UTC kind.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithIso8601ZSuffixReturnsUtcKindAsync`
**Reason:** Verify that `"2025-12-31T10:00:00Z"` is parsed with `DateTimeKind.Utc` and not
double-converted through timezone logic.

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithIso8601OffsetConvertsToUtcAsync`
**Reason:** Verify that `"2025-12-31T10:00:00-05:00"` is correctly converted to
`2025-12-31T15:00:00Z`.

---

### 4 - Handle time-only formats by anchoring to reference date (Comment 9)

Time-only format strings like `"h:mm tt"`, `"HH:mm"`, `"h tt"` in `ExactFormats` produce a
`DateTime` with the default date (year 0001). These should be removed from `ExactFormats` since
they are already handled by the fuzzy parser's `ClockTimeParser` (which anchors to today's date).
Alternatively, detect time-only results and anchor them to `now.Date`.

### Files

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Remove time-only formats from `ExactFormats` array (lines 34-38: `"h:mm tt"`, `"hh:mm tt"`, `"HH:mm"`, `"h tt"`)
**Reason:** Time-only inputs are already handled by the fuzzy parser in Step 1 (via
`ClockTimeParser`). Leaving them in `ExactFormats` creates a fallthrough path that produces
dates in year 0001. If the fuzzy parser misses a time-only input, it should fall through to
the LLM rather than produce a nonsensical date.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithTimeOnlyInputIsHandledByFuzzyParserAsync`
**Reason:** Verify that `"3:00 PM"` is handled by the fuzzy parser (mocked to succeed with a
sensible date) and not by the exact format fallback. Update existing test
`ParseTimeAsyncWithCommonTimeFormatReturnsUtcDateTime` to reflect the new behavior.

---

### 5 - Guard `FindSystemTimeZoneById` against invalid timezone IDs (Comment 2)

`ConvertUnspecifiedToUtc` calls `TimeZoneInfo.FindSystemTimeZoneById(userTimeZoneId)` which
can throw `TimeZoneNotFoundException` or `InvalidTimeZoneException`. This violates the
`Result<T>` error-handling convention.

### Files

#### `src/Apollo.Application/ToDos/TimeParsingService.cs`: Wrap `FindSystemTimeZoneById` in try/catch, falling back to UTC
**Reason:** Invalid timezone IDs should not cause unhandled exceptions. On failure, treat the
parsed time as UTC (same behavior as when no timezone is provided). Catch both
`TimeZoneNotFoundException` and `InvalidTimeZoneException`.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithInvalidTimezoneIdFallsBackToUtcAsync`
**Reason:** Verify that an invalid timezone ID like `"Invalid/Timezone"` does not throw an
exception and instead falls back to treating the time as UTC.

#### `tests/Apollo.Application.Tests/ToDos/TimeParsingServiceTests.cs`: `ParseTimeAsyncWithEmptyTimezoneIdTreatsAsUtcAsync`
**Reason:** Verify that an empty or whitespace timezone ID is treated the same as null (UTC).

---

### 6 - Restrict bare hour parsing to 13–23 range (Comment 10)

`TimeParserHelpers.ParseTimeOfDay` accepts bare integers 0–23 as hours. A bare `"3"` becomes
03:00 AM, which is almost certainly not what the user meant when saying "at 3" or "tomorrow at 3".
Restrict bare hour parsing to the unambiguous 13–23 range; hours 0–12 should require AM/PM.

### Files

#### `src/Apollo.Application/ToDos/Parsers/TimeParserHelpers.cs`: Change `hour is >= 0 and <= 23` to `hour is >= 13 and <= 23`
**Reason:** Bare hours 0–12 are ambiguous (could be AM or PM). Restricting to 13–23 ensures
only unambiguous 24-hour values are accepted. Ambiguous inputs like "at 3" will fail the fuzzy
parser and fall through to the LLM which can use conversation context to disambiguate.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: `TryParseFuzzyTimeWithBareHour3DoesNotParseAs3AmAsync`
**Reason:** Verify that "at 3" (bare hour < 13) is NOT parsed by the fuzzy parser (returns failure).

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: `TryParseFuzzyTimeWithBareHour15ParsesAs1500Async`
**Reason:** Verify that "at 15" (bare hour ≥ 13) IS parsed as 15:00.

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: Update existing "tomorrow at 3" test expectations
**Reason:** Tests that rely on "tomorrow at 3" resolving to 03:00 need to be updated — they should
now expect a failure (bare 3 is ambiguous) while "tomorrow at 3pm" and "tomorrow at 15" still work.

---

### 7 - Add "tomorrow morning/afternoon/evening" to `TomorrowParser` (Comments 3, 6)

The prompts list "tomorrow morning" as a supported format, but no deterministic parser handles it.
Rather than removing it from the prompts (which reduces user-friendliness), add support to
`TomorrowParser` for "tomorrow morning", "tomorrow afternoon", and "tomorrow evening" using the
same time-of-day mappings as `TimeOfDayAliasParser`.

### Files

#### `src/Apollo.Application/ToDos/Parsers/TomorrowParser.cs`: Add a new regex pattern `TomorrowPeriodPattern` matching `tomorrow morning|afternoon|evening`
**Reason:** "tomorrow morning" is a natural expression users will say. Adding deterministic
support avoids unnecessary LLM fallback calls and latency. Map to the same hours as
`TimeOfDayAliasParser`: morning=09:00, afternoon=14:00, evening=18:00.

### Tests

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: `TryParseFuzzyTimeWithTomorrowMorningReturnsNextDay9Am`
**Reason:** Verify "tomorrow morning" resolves to tomorrow at 09:00 UTC.

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: `TryParseFuzzyTimeWithTomorrowAfternoonReturnsNextDay2Pm`
**Reason:** Verify "tomorrow afternoon" resolves to tomorrow at 14:00 UTC.

#### `tests/Apollo.Application.Tests/ToDos/FuzzyTimeParserTests.cs`: `TryParseFuzzyTimeWithTomorrowEveningReturnsNextDay6Pm`
**Reason:** Verify "tomorrow evening" resolves to tomorrow at 18:00 UTC.

---

### 8 - Pass user timezone in `ApolloGrpcService.CreateReminderAsync` (Comment 8)

The gRPC service calls `ParseTimeAsync(request.ReminderTime)` without passing the user's timezone,
so user-local clock times like "at 3pm" are interpreted as UTC. The `Person` object is available
via `userContext.Person!` and has a `TimeZoneId` property.

### Files

#### `src/Apollo.GRPC/Service/ApolloGrpcService.cs`: Extract person's timezone and pass to `ParseTimeAsync`
**Reason:** Without the timezone, all user-local time expressions are interpreted as UTC,
causing reminders to fire at the wrong time. Derive timezone from
`person.TimeZoneId?.ToString()` (or equivalent) and pass as the second argument.

### Tests

#### `tests/Apollo.GRPC.Tests/ApolloGrpcServiceTests.cs`: `CreateReminderAsyncPassesUserTimezoneToTimeParsingServiceAsync`
**Reason:** Verify that when a person has a timezone set, it is forwarded to `ParseTimeAsync`.

#### `tests/Apollo.GRPC.Tests/ApolloGrpcServiceTests.cs`: `CreateReminderAsyncWithNoTimezonePassesNullToTimeParsingServiceAsync`
**Reason:** Verify that when a person has no timezone, `null` is passed (default behavior).
