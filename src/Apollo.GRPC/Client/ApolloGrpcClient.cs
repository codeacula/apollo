using Apollo.Core.API;
using Apollo.Core.ToDos.Responses;
using Apollo.Domain.Common.Enums;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using FluentResults;

using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using ProtoBuf.Grpc.Client;

using CoreCreateToDoRequest = Apollo.Core.ToDos.Requests.CreateToDoRequest;
using CoreNewMessageRequest = Apollo.Core.Conversations.NewMessageRequest;
using GrpcCreateToDoRequest = Apollo.GRPC.Contracts.CreateToDoRequest;
using GrpcGetPersonToDosRequest = Apollo.GRPC.Contracts.GetPersonToDosRequest;
using GrpcNewMessageRequest = Apollo.GRPC.Contracts.NewMessageRequest;
using GrpcToDoDTO = Apollo.GRPC.Contracts.ToDoDTO;

namespace Apollo.GRPC.Client;

public class ApolloGrpcClient : IApolloGrpcClient, IApolloServiceClient, IDisposable
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
      Platform = request.PlatformId.Platform,
      PlatformUserId = request.PlatformId.PlatformUserId,
      Username = request.PlatformId.Username,
      Title = request.Title,
      Description = request.Description,
      ReminderDate = request.ReminderDate,
    };

    Result<GrpcToDoDTO> grpcResponse = await ApolloGrpcService.CreateToDoAsync(grpcRequest);

    return grpcResponse.IsFailed ? Result.Fail<ToDo>(grpcResponse.Errors) : Result.Ok(MapToDomain(grpcResponse.Value));
  }

  public async Task<Result<string>> SendMessageAsync(CoreNewMessageRequest request)
  {
    var grpcRequest = new GrpcNewMessageRequest
    {
      Platform = request.PlatformId.Platform,
      PlatformUserId = request.PlatformId.PlatformUserId,
      Username = request.PlatformId.Username,
      Content = request.Content
    };

    var grpcResult = await ApolloGrpcService.SendApolloMessageAsync(grpcRequest);

    return grpcResult.IsSuccess ?
      Result.Ok(grpcResult.Data ?? string.Empty) :
      Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }

  public async Task<Result<IEnumerable<ToDoSummary>>> GetToDosAsync(PlatformId platformId, bool includeCompleted = false)
  {
    var grpcRequest = new GrpcGetPersonToDosRequest
    {
      Platform = platformId.Platform,
      IncludeCompleted = includeCompleted,
      PlatformUserId = platformId.PlatformUserId,
      Username = platformId.Username
    };

    Result<GrpcToDoDTO[]> grpcResponse = await ApolloGrpcService.GetPersonToDosAsync(grpcRequest);

    if (grpcResponse.IsFailed)
    {
      return Result.Fail<IEnumerable<ToDoSummary>>(grpcResponse.Errors);
    }

    var summaries = grpcResponse.Value.Select(dto => new ToDoSummary
    {
      Id = dto.Id,
      Description = dto.Description,
      ReminderDate = dto.ReminderDate,
      CreatedOn = dto.CreatedOn,
      UpdatedOn = dto.UpdatedOn
    }).ToArray();

    return Result.Ok<IEnumerable<ToDoSummary>>(summaries);
  }

  private static ToDo MapToDomain(GrpcToDoDTO dto)
  {
    return new ToDo
    {
      Id = new(dto.Id),
      PersonId = new(dto.PersonId),
      Description = new(dto.Description),
      Priority = new(Level.Blue),
      Energy = new(Level.Blue),
      Interest = new(Level.Blue),
      Reminders = [],
      DueDate = null,
      CreatedOn = new(dto.CreatedOn),
      UpdatedOn = new(dto.UpdatedOn)
    };
  }
}
