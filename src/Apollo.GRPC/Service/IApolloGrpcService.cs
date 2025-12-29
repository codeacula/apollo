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
  Task<GrpcResult<ToDoDTO>> CreateToDoAsync(CreateToDoRequest request);

  [OperationContract]
  Task<GrpcResult<ToDoDTO>> GetToDoAsync(GetToDoRequest request);

  [OperationContract]
  Task<GrpcResult<ToDoDTO[]>> GetPersonToDosAsync(GetPersonToDosRequest request);

  [OperationContract]
  Task<GrpcResult<string>> UpdateToDoAsync(UpdateToDoRequest request);

  [OperationContract]
  Task<GrpcResult<string>> CompleteToDoAsync(CompleteToDoRequest request);

  [OperationContract]
  Task<GrpcResult<string>> DeleteToDoAsync(DeleteToDoRequest request);
}
