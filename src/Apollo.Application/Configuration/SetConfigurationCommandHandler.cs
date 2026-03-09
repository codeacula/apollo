using Apollo.Core.Configuration;
using FluentResults;
using MediatR;
using StackExchange.Redis;

namespace Apollo.Application.Configuration;

public class SetConfigurationCommandHandler(
  IConfigurationStore store,
  IConnectionMultiplexer redis) : IRequestHandler<SetConfigurationCommand, Result>
{
  public async Task<Result> Handle(SetConfigurationCommand request, CancellationToken cancellationToken)
  {
    var result = await store.SetConfigurationAsync(request.Key, request.Value, cancellationToken);
    if (result.IsSuccess)
    {
      await redis.GetSubscriber().PublishAsync(RedisChannel.Literal("apollo:config:updates"), $"SET:{request.Key}");
    }
    return result;
  }
}
