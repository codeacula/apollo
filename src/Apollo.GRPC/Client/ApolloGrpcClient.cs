using Apollo.Core.API;
using Apollo.Core.ToDos.Responses;
using Apollo.Domain.People.ValueObjects;
using Apollo.Domain.ToDos.Models;
using Apollo.GRPC.Interceptors;
using Apollo.GRPC.Service;

using FluentResults;

using Grpc.Core.Interceptors;
using Grpc.Net.Client;

using ProtoBuf.Grpc.Client;

using CoreCreateReminderRequest = Apollo.Core.Reminders.Requests.CreateReminderRequest;
using CoreCreateToDoRequest = Apollo.Core.ToDos.Requests.CreateToDoRequest;
using CoreNewMessageRequest = Apollo.Core.Conversations.NewMessageRequest;
using GrpcCreateReminderRequest = Apollo.GRPC.Contracts.CreateReminderRequest;
using GrpcCreateToDoRequest = Apollo.GRPC.Contracts.CreateToDoRequest;
using GrpcGetDailyPlanRequest = Apollo.GRPC.Contracts.GetDailyPlanRequest;
using GrpcGetPersonToDosRequest = Apollo.GRPC.Contracts.GetPersonToDosRequest;
using GrpcManageAccessRequest = Apollo.GRPC.Contracts.ManageAccessRequest;
using GrpcNewMessageRequest = Apollo.GRPC.Contracts.NewMessageRequest;
using GrpcReminderDTO = Apollo.GRPC.Contracts.ReminderDTO;
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

  public async Task<Result<ToDo>> CreateToDoAsync(CoreCreateToDoRequest request, CancellationToken cancellationToken = default)
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

  public async Task<Result<Reminder>> CreateReminderAsync(CoreCreateReminderRequest request, CancellationToken cancellationToken = default)
  {
    var grpcRequest = new GrpcCreateReminderRequest
    {
      Platform = request.PlatformId.Platform,
      PlatformUserId = request.PlatformId.PlatformUserId,
      Username = request.PlatformId.Username,
      Message = request.Message,
      ReminderTime = request.ReminderTime,
    };

    Result<GrpcReminderDTO> grpcResponse = await ApolloGrpcService.CreateReminderAsync(grpcRequest);

    return grpcResponse.IsFailed
      ? Result.Fail<Reminder>(grpcResponse.Errors)
      : Result.Ok(MapReminderToDomain(grpcResponse.Value));
  }

  public async Task<Result<string>> SendMessageAsync(CoreNewMessageRequest request, CancellationToken cancellationToken = default)
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

  public async Task<Result<IEnumerable<ToDoSummary>>> GetToDosAsync(PlatformId platformId, bool includeCompleted = false, CancellationToken cancellationToken = default)
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

  public async Task<Result<DailyPlanResponse>> GetDailyPlanAsync(PlatformId platformId, CancellationToken cancellationToken = default)
  {
    var grpcRequest = new GrpcGetDailyPlanRequest
    {
      Platform = platformId.Platform,
      PlatformUserId = platformId.PlatformUserId,
      Username = platformId.Username
    };

    var grpcResponse = await ApolloGrpcService.GetDailyPlanAsync(grpcRequest);

    if (!grpcResponse.IsSuccess || grpcResponse.Data == null)
    {
      return Result.Fail<DailyPlanResponse>(
        string.Join("; ", grpcResponse.Errors.Select(e => e.Message))
      );
    }

    var dto = grpcResponse.Data;
    var tasks = dto.SuggestedTasks.Select(t => new DailyPlanTaskResponse
    {
      Id = t.Id,
      Description = t.Description,
      Priority = t.Priority,
      Energy = t.Energy,
      Interest = t.Interest,
      DueDate = t.DueDate
    }).ToList();

    var response = new DailyPlanResponse
    {
      SuggestedTasks = tasks,
      SelectionRationale = dto.SelectionRationale,
      TotalActiveTodos = dto.TotalActiveTodos
    };

    return Result.Ok(response);
  }

  public async Task<Result<string>> GrantAccessAsync(PlatformId adminPlatformId, PlatformId targetPlatformId, CancellationToken cancellationToken = default)
  {
    var grpcRequest = new GrpcManageAccessRequest
    {
      AdminPlatform = adminPlatformId.Platform,
      AdminPlatformUserId = adminPlatformId.PlatformUserId,
      AdminUsername = adminPlatformId.Username,
      TargetPlatform = targetPlatformId.Platform,
      TargetPlatformUserId = targetPlatformId.PlatformUserId,
      TargetUsername = targetPlatformId.Username
    };

    var grpcResult = await ApolloGrpcService.GrantAccessAsync(grpcRequest);

    return grpcResult.IsSuccess
      ? Result.Ok(grpcResult.Data ?? string.Empty)
      : Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }

  public async Task<Result<string>> RevokeAccessAsync(PlatformId adminPlatformId, PlatformId targetPlatformId, CancellationToken cancellationToken = default)
  {
    var grpcRequest = new GrpcManageAccessRequest
    {
      AdminPlatform = adminPlatformId.Platform,
      AdminPlatformUserId = adminPlatformId.PlatformUserId,
      AdminUsername = adminPlatformId.Username,
      TargetPlatform = targetPlatformId.Platform,
      TargetPlatformUserId = targetPlatformId.PlatformUserId,
      TargetUsername = targetPlatformId.Username
    };

    var grpcResult = await ApolloGrpcService.RevokeAccessAsync(grpcRequest);

    return grpcResult.IsSuccess
      ? Result.Ok(grpcResult.Data ?? string.Empty)
      : Result.Fail(string.Join("; ", grpcResult.Errors.Select(e => e.Message)));
  }

  private static ToDo MapToDomain(GrpcToDoDTO dto)
  {
    return new ToDo
    {
      Id = new(dto.Id),
      PersonId = new(dto.PersonId),
      Description = new(dto.Description),
      Priority = new(dto.Priority),
      Energy = new(dto.Energy),
      Interest = new(dto.Interest),
      Reminders = [],
      DueDate = null,
      CreatedOn = new(dto.CreatedOn),
      UpdatedOn = new(dto.UpdatedOn)
    };
  }

  private static Reminder MapReminderToDomain(GrpcReminderDTO dto)
  {
    return new Reminder
    {
      Id = new(dto.Id),
      PersonId = new(dto.PersonId),
      Details = new(dto.Details),
      ReminderTime = new(dto.ReminderTime),
      CreatedOn = new(dto.CreatedOn),
      UpdatedOn = new(dto.UpdatedOn)
    };
  }
}
