using Apollo.GRPC.Actions;

namespace Apollo.GRPC.Service;

public class ApolloGrpcService : IApolloGrpcService
{
  public Task<GrpcResult<string>> SendApolloMessageAsync(string message)
  {
    // Implement the method logic here
    throw new NotImplementedException();
  }
}
