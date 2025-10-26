---
mode: 'edit'
---
# Create GRPC Interaction

## Needed Variables

- **Domain**: The domain of the interaction, such as `Ai`, `Game`, `User`, etc.
- **Interaction**: The name of the interaction, such as `TalkWithGm`, `GreetGm`, etc.

## Prompt

Create a gRPC interaction based on the domain and interaction provided, remembering to follow the coding and AI agent guidelines. Before performing any action, ensure you have the necessary information to continue - if you don't, ask the user for it. Then, follow these steps:

1. Create a folder: `src/Apollo.Application/Grpc/{Domain}/{Interaction}`.
1. Create `src/Apollo.Application/Grpc/{Domain}/{Interaction}/{Interaction}Response.cs`:

    ```csharp
    namespace Apollo.Application.Grpc.Ai.{Interaction};

    [ProtoContract]
    public sealed record {Interaction}Response
    {
      [ProtoMember(1)]
      public string Response { get; init; } = string.Empty;

      public {Interaction}Response() { }

      public {Interaction}Response(string response)
      {
        Response = response;
      }
    }
    ```

1. Create `src/Apollo.Application/Grpc/{Domain}/{Interaction}/{Interaction}Result.cs`.

    ```csharp
    namespace Apollo.Application.Grpc.Ai.{Interaction};

    public sealed record {Interaction}Result(string Message);
    ```

1. Create `src/Apollo.Application/Grpc/{Domain}/{Interaction}/{Interaction}Request.cs`.

    ```csharp
    namespace Apollo.Application.Grpc.Ai.{Interaction};

    [ProtoContract]
    public sealed record {Interaction}Request : IRequest<{Interaction}Response>
    {
      [ProtoMember(1)]
      public string InteractionText { get; init; } = string.Empty;

      public {Interaction}Request() { }

      public {Interaction}Request(string interactionText)
      {
        InteractionText = interactionText;
      }
    }
    ```

1. Create `src/Apollo.Application/Grpc/{Domain}/{Interaction}/{Interaction}Command.cs`.

    ```csharp
    namespace Apollo.Application.Grpc.Ai.{Interaction};

    public sealed record {Interaction}Command(string InteractionText) : IRequest<{Interaction}Response>;
    ```

1. Create `src/Apollo.Application/Grpc/{Domain}/{Interaction}/{Interaction}Handler.cs`.

    ```csharp
    using Microsoft.Extensions.Logging;

    namespace Apollo.Application.Grpc.Ai.{Interaction};

    internal sealed partial class {Interaction}Handler(ILogger<{Interaction}Handler> logger) : IRequestHandler<{Interaction}Request, {Interaction}Response>
    {
      private readonly ILogger<{Interaction}Handler> _logger = logger;
      public async Task<{Interaction}Response> Handle({Interaction}Request request, CancellationToken cancellationToken)
      {
        ProcessingRequest(_logger, request.InteractionText.Length);

        try
        {
            return new {Interaction}Response(response);
        }
        catch (Exception ex)
        {
          ErrorProcessingRequest(_logger, ex);
          throw new ApolloInvalidResponseException("An error occurred while processing the request.", ex);
        }
      }

      [LoggerMessage(Level = LogLevel.Information, Message = "Processing conversation request: {MessageLength} chars")]
      public static partial void ProcessingRequest(ILogger logger, int messageLength);

      [LoggerMessage(Level = LogLevel.Error, Message = "Error processing conversation request")]
      public static partial void ErrorProcessingRequest(ILogger logger, Exception exception);
    }
    ```
