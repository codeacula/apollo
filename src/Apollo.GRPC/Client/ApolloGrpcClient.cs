using Apollo.Core.API;
using Apollo.Core.Conversations;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.Domain.ToDos.ValueObjects;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using FluentResults;

using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using ProtoBuf.Grpc.Client;

using CoreCreateToDoRequest = Apollo.Core.ToDos.Requests.CreateToDoRequest;
using GrpcCreateToDoRequest = Apollo.GRPC.Contracts.CreateToDoRequest;
using GrpcToDoDTO = Apollo.GRPC.Contracts.ToDoDTO;

namespace Apollo.GRPC.Client;

public class ApolloGrpcClient : IApolloGrpcClient, IApolloAPIClient, IDisposable
{
  public IApolloGrpcService ApolloGrpcService { get; }
  private readonly GrpcChannel _channel;

  public ApolloGrpcClient(GrpcChannel channel, GrpcClientLoggingInterceptor GrpcClientLoggingInterceptor, GrpcHostConfig grpcHostConfig)
  {
    _channel = channel;
    var invoker = _channel.Intercept(GrpcClientLoggingInterceptor)
      .Intercept(metadata =>
      {
        metadata.Add("X-API-Token", grpcHostConfig.ApiToken);
        return metadata;
      });
    ApolloGrpcService = invoker.CreateGrpcService<IApolloGrpcService>();
  }

  public void Dispose()
  {
    _channel.Dispose();
    GC.SuppressFinalize(this);
  }

  public async Task<Result<ToDo>> CreateToDoAsync(CoreCreateToDoRequest request)
  {
    var grpcRequest = new GrpcCreateToDoRequest
    {
      Username = request.Username,
      Platform = request.Platform,
      Description = request.Description,
      ReminderDate = request.ReminderDate
    };

    Result<GrpcToDoDTO> grpcResponse = await ApolloGrpcService.CreateToDoAsync(grpcRequest);

    return grpcResponse.IsFailed ? Result.Fail<ToDo>(grpcResponse.Errors) : Result.Ok(MapToDomain(grpcResponse.Value));
  }

  public async Task<Result<string>> SendMessageAsync(NewMessageRequest request)
  {
    var grpcResult = await ApolloGrpcService.SendApolloMessageAsync(request);

    return grpcResult.IsSuccess ?
      Result.Ok(grpcResult.Data ?? string.Empty) :
      Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }

  private static ToDo MapToDomain(GrpcToDoDTO dto)
  {
    return new ToDo
    {
      Id = new ToDoId(dto.Id),
      PersonId = new PersonId(dto.PersonId),
      Description = new Description(dto.Description),
      Priority = new Priority(Level.Blue),
      Energy = new Energy(Level.Blue),
      Interest = new Interest(Level.Blue),
      Reminders = [],
      DueDate = null,
      CreatedOn = new CreatedOn(dto.CreatedOn),
      UpdatedOn = new UpdatedOn(dto.UpdatedOn)
    };
  }
}
