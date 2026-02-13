namespace Apollo.GRPC.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class RequireAccessAttribute : Attribute;
