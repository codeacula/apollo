using System.Runtime.Serialization;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetDailyPlanRequest : AuthenticatedRequestBase
{
}
