using System.ServiceModel;

using Apollo.Core.Conversations;
using Apollo.GRPC.Contracts;

namespace Apollo.GRPC.Service;

[ServiceContract]
public interface IApolloGrpcService
{
  [OperationContract]
  Task<GrpcResult<string>> SendApolloMessageAsync(NewMessage message);

  [OperationContract]
  Task<GrpcResult<ToDoDto>> CreateToDoAsync(CreateToDoRequest request);

  [OperationContract]
  Task<GrpcResult<ToDoDto>> GetToDoAsync(Guid todoId);

  [OperationContract]
  Task<GrpcResult<ToDoDto[]>> GetPersonToDosAsync(Guid personId);

  [OperationContract]
  Task<GrpcResult<string>> UpdateToDoAsync(UpdateToDoRequest request);

  [OperationContract]
  Task<GrpcResult<string>> CompleteToDoAsync(Guid todoId);

  [OperationContract]
  Task<GrpcResult<string>> DeleteToDoAsync(Guid todoId);
}
