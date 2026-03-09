using NetCord.Services.ComponentInteractions;

namespace Apollo.Discord.TypeReaders;

public sealed class GuidTypeReader<TContext> : ComponentInteractionTypeReader<TContext>
  where TContext : IComponentInteractionContext
{
  public override ValueTask<ComponentInteractionTypeReaderResult> ReadAsync(
    ReadOnlyMemory<char> input,
    TContext context,
    ComponentInteractionParameter<TContext> parameter,
    ComponentInteractionServiceConfiguration<TContext> configuration,
    IServiceProvider? serviceProvider)
  {
    return Guid.TryParse(input.Span, out var guid)
      ? ValueTask.FromResult(ComponentInteractionTypeReaderResult.Success(guid))
      : ValueTask.FromResult(ComponentInteractionTypeReaderResult.ParseFail(parameter.Name));
  }
}
