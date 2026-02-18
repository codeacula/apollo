namespace Apollo.Core.ToDos;

/// <summary>
/// Marks a class as an <see cref="ITimeExpressionParser"/> implementation that should be
/// automatically discovered and registered with <see cref="IFuzzyTimeParser"/> at startup.
/// Any class decorated with this attribute that also implements <see cref="ITimeExpressionParser"/>
/// will be instantiated via reflection and included in the parsing chain.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class TimeExpressionParserAttribute : Attribute;
