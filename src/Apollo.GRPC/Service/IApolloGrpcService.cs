using System.ServiceModel;

using Apollo.GRPC.Attributes;
using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IApolloGrpcService
{
  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<string>> SendApolloMessageAsync(NewMessageRequest message);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<ToDoDTO>> CreateToDoAsync(CreateToDoRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<ReminderDTO>> CreateReminderAsync(CreateReminderRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<ToDoDTO>> GetToDoAsync(GetToDoRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<ToDoDTO[]>> GetPersonToDosAsync(GetPersonToDosRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<DailyPlanDTO>> GetDailyPlanAsync(GetDailyPlanRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<string>> UpdateToDoAsync(UpdateToDoRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<string>> CompleteToDoAsync(CompleteToDoRequest request);

  [OperationContract]
  [RequireAccess]
  Task<GrpcResult<string>> DeleteToDoAsync(DeleteToDoRequest request);

  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<string>> GrantAccessAsync(ManageAccessRequest request);

  [OperationContract]
  [RequireSuperAdmin]
  Task<GrpcResult<string>> RevokeAccessAsync(ManageAccessRequest request);
}
