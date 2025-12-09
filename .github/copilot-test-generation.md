# GitHub Copilot - Test Generation Guidelines for Apollo (Using Moq)

You are generating unit tests for a C# class in the Apollo project. Your goal is to create comprehensive, readable, and maintainable tests using Moq as the mocking framework.

1. **Primary Guidance**:
   - The definitive testing philosophy, conventions (including AAA pattern, naming `MethodUnderTestGivenConditionShouldExpectedResult`), tooling (Moq, FluentAssertions, AutoFixture), and general best practices are detailed in the testing section of `.github/copilot-instructions.md`. You MUST adhere to all rules and guidelines in that section.
   - This document provides specific reminders and focus points for test generation with Moq.

2. **Core Test Generation Requirements**:
   - **Identify Testable Units**: When given a C# class, identify all public methods. Each distinct logical path, condition, or outcome for these methods should be a candidate for a separate test method.
   - **Test Class Structure**:
       - Name the test class `<ClassNameUnderTest>Tests.cs` (e.g., `MyServiceTests.cs` for `MyService.cs`).
       - Place the test file in the correct test project, mirroring the source file's folder structure (e.g., tests for `src/Apollo.Application/Services/MyService.cs` go into `tests/Apollo.Tests.Application/Services/MyServiceTests.cs`).
       - Include necessary usings: `Xunit`, `FluentAssertions`, `Moq`, `AutoFixture`, and the namespace of the class under test.
       - Typically, include private readonly fields for Moq `Mock<IDependency>` objects and a field for the System Under Test (SUT).
       - Initialize `Mock` objects and the SUT in the test class constructor. `AutoFixture` can be helpful here for creating the SUT or concrete dependencies. Pass `mockDependency.Object` to the SUT's constructor.
   - **Individual Test Method Structure (AAA Pattern)**:
       - **Arrange**:
           - Set up all preconditions.
           - Create mock dependencies using `var mockDependency = new Mock<IDependency>();`.
           - Configure their behavior using `mockDependency.Setup(m => m.SomeMethod(It.IsAny<string>())).Returns(expectedValue);` or `...ReturnsAsync(...)` for async methods.
           - Use `AutoFixture` to generate test data (DTOs, primitive types, etc.) to keep tests concise and focused on the behavior, not data setup.
       - **Act**:
           - Execute the method under test on the SUT (which was instantiated with `mockDependency.Object`) with the arranged inputs.
       - **Assert**:
           - Use `FluentAssertions` for all assertions (e.g., `result.Should().BeTrue()`, `actualObject.Should().BeEquivalentTo(expectedObject)`).
           - Verify expected outcomes and state changes.
           - If testing methods that return `Result<TSuccess, TError>`, assert `IsSuccess` or `IsFailure` and check the `Value` or `Error` accordingly.
           - Verify calls to mock dependencies if the interaction is a key part of the behavior being tested, using `mockDependency.Verify(m => m.SomeMethodAsync(expectedArg), Times.Once());`.

3. **Test Coverage Focus**:
   - **Happy Paths**: Test the expected behavior with valid inputs.
   - **Edge Cases**: Test boundary conditions, null inputs (where applicable using `It.IsNotNull<T>()` or specific checks), empty collections, etc.
   - **Error Conditions/`Result` States**: For methods returning `Result<T>`, ensure tests for both success and failure paths. If exceptions are expected (and not handled by `Result`), test for those using `FluentActions.Invoking(() => ...).Should().Throw<SomeException>()`.
   - **Different SUT States**: If the SUT's behavior changes based on its internal state, test these variations.

4. **Tooling Reminders**:
   - **Moq**: For all mocks and stubs. Use `It.IsAny<T>()`, `It.IsNotNull<T>()` for argument matching. Use `Times.Exactly(n)`, `Times.Once()`, `Times.Never()` for verification.
   - **FluentAssertions**: For all assertions.
   - **AutoFixture**: For generating test data and sometimes for creating SUTs.
   - **xUnit**: Use `[Fact]` for parameterless tests and `[Theory]` with `[InlineData]`, `[MemberData]`, or `[ClassData]` for data-driven tests where appropriate (ensure readability).

5. **What to Avoid**:
   - Overly complex test logic. Each test should be simple and focused.
   - Mocking concrete types you don't own if not necessary (prefer interfaces or abstractions). `Moq` primarily mocks interfaces or virtual members of classes.
   - Asserting multiple distinct behaviors in a single test method (unless very tightly coupled).

6. **Testing AI Components**:
   - **Semantic Kernel Testing**:
       - Mock the `IKernel` or specific plugin interfaces rather than using actual AI services in unit tests
       - Use dependency injection to replace AI components with test doubles that return predictable responses
       - Test the structure and processing of AI responses, not just the fact that a response was received
       - For prompt testing, verify the correct context variables are populated before being sent to SK
   - **Deterministic vs. Non-deterministic Testing**:
       - Unit tests should focus on deterministic aspects (correct API calls, proper error handling)
       - Consider separate integration test suites for testing actual AI responses with controlled prompts

7. **Testing Event-Sourced Aggregates**:
   - **Event Application Testing**:
       - Test that aggregates correctly apply events and update their state
       - Verify events are generated with the correct data when commands are processed
       - Test aggregate reconstitution from event streams
   - **Projection Testing**:
       - Verify projections correctly transform events into read models
       - Test edge cases like out-of-order events or duplicate events

8. **Security-Focused Testing**:
   - Test input validation and sanitization, especially for player-provided content
   - Verify authorization checks are correctly applied where required
   - Test error handling to ensure it doesn't reveal sensitive information

**Example Snippet (Conceptual Structure of a Test Method with Moq):**

```csharp
[Fact]
public async Task MethodUnderTestGivenSpecificConditionShouldReturnExpectedResult()
{
    // Arrange
    var fixture = new Fixture(); // AutoFixture instance if not a field
    var inputValue = fixture.Create<string>();
    var expectedDto = fixture.Create<ExpectedDto>();

    var mockDependency = new Mock<IDependency>();
    mockDependency.Setup(m => m.GetDataAsync(inputValue)).ReturnsAsync(expectedDto);

    var sut = new SystemUnderTest(mockDependency.Object); // SUT gets the mocked object

    // Act
    var result = await sut.MethodUnderTestAsync(inputValue);

    // Assert
    result.Should().NotBeNull();
    // Assuming Result<T> pattern
    // result.IsSuccess.Should().BeTrue();
    // result.Value.Should().BeEquivalentTo(expectedDto); // FluentAssertions

    mockDependency.Verify(m => m.GetDataAsync(inputValue), Times.Once()); // Moq verification
}
```
