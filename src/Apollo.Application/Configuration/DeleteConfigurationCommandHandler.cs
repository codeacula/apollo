using Apollo.Core.Configuration;
using FluentResults;
using MediatR;
using StackExchange.Redis;

namespace Apollo.Application.Configuration;

public class DeleteConfigurationCommandHandler(
  IConfigurationStore store,
  IConnectionMultiplexer redis) : IRequestHandler<DeleteConfigurationCommand, Result>
{
  public async Task<Result> Handle(DeleteConfigurationCommand request, CancellationToken cancellationToken)
  {
    var result = await store.DeleteConfigurationAsync(request.Key, cancellationToken);
    if (result.IsSuccess)
    {
      await redis.GetSubscriber().PublishAsync(RedisChannel.Literal("apollo:config:updates"), $"DEL:{request.Key}");
    }
    return result;
  }
}
