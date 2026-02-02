using System.Runtime.Serialization;

using Apollo.Domain.Common.Enums;

namespace Apollo.GRPC.Contracts;

[DataContract]
public sealed record GetDailyPlanRequest : AuthenticatedRequestBase
{
}
