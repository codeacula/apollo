using Apollo.AI;
using Apollo.AI.Models;
using Apollo.AI.Planning;
using Apollo.AI.Requests;
using Apollo.AI.Tooling;
using Apollo.Application.People;
using Apollo.Application.Reminders;
using Apollo.Application.ToDos;
using Apollo.Core;
using Apollo.Core.Conversations;
using Apollo.Core.Logging;
using Apollo.Core.People;
using Apollo.Core.ToDos;
using Apollo.Domain.Common.ValueObjects;
using Apollo.Domain.Conversations.Models;
using Apollo.Domain.People.Models;
using Apollo.Domain.People.ValueObjects;

using FluentResults;

namespace Apollo.Application.Conversations;

/// <summary>
/// Tells the system to process an incoming message from a supported platform.
/// </summary>
/// <param name="PersonId">The ID of the person sending the message.</param>
/// <param name="Content">The message content.</param>
/// <seealso cref="ProcessIncomingMessageCommandHandler"/>
public sealed record ProcessIncomingMessageCommand(PersonId PersonId, Content Content) : IRequest<Result<Reply>>;

public sealed class ProcessIncomingMessageCommandHandler(
  IApolloAIAgent apolloAIAgent,
  IConversationStore conversationStore,
  ITimeParsingService timeParsingService,
  ILogger<ProcessIncomingMessageCommandHandler> logger,
  IMediator mediator,
  IPersonStore personStore,
  IToDoStore toDoStore,
  PersonConfig personConfig,
  TimeProvider timeProvider
) : IRequestHandler<ProcessIncomingMessageCommand, Result<Reply>>
{
  public async Task<Result<Reply>> Handle(ProcessIncomingMessageCommand request, CancellationToken cancellationToken = default)
  {
    try
    {
      var personResult = await personStore.GetAsync(request.PersonId, cancellationToken);
      if (personResult.IsFailed)
      {
        return Result.Fail<Reply>($"Failed to load person {request.PersonId.Value}");
      }

      var person = personResult.Value;

      var conversationResult = await GetOrCreateConversationWithMessageAsync(person, request.Content.Value, cancellationToken);
      if (conversationResult.IsFailed)
      {
        return conversationResult.ToResult<Reply>();
      }

      var response = await ProcessWithAIAsync(conversationResult.Value, person, cancellationToken);

      await SaveReplyAsync(conversationResult.Value, response, cancellationToken);

      return CreateReplyToUser(response);
    }
    catch (Exception ex)
    {
      DataAccessLogs.UnhandledMessageProcessingError(logger, ex, request.PersonId.Value.ToString());
      return Result.Fail<Reply>(ex.Message);
    }
  }

  private async Task<Result<Conversation>> GetOrCreateConversationWithMessageAsync(Person person, string messageContent, CancellationToken cancellationToken)
  {
    var convoResult = await conversationStore.GetOrCreateConversationByPersonIdAsync(person.Id, cancellationToken);

    if (convoResult.IsFailed)
    {
      return Result.Fail<Conversation>("Unable to fetch conversation.");
    }

    convoResult = await conversationStore.AddMessageAsync(convoResult.Value.Id, new Content(messageContent), cancellationToken);

    return convoResult.IsFailed ? Result.Fail<Conversation>("Unable to add message to conversation.") : convoResult;
  }

  private async Task<string> ProcessWithAIAsync(Conversation conversation, Person person, CancellationToken cancellationToken)
  {
    var responseMessages = ConversationHistoryBuilder.BuildForResponse(conversation);
    var plugins = CreatePlugins(person);

    // Get context variables
    var userTimezone = GetUserTimezone(person);
    var activeTodosSnapshot = await BuildActiveTodosSnapshotAsync(person.Id, cancellationToken);
    var toolPlanningMessages = ConversationHistoryBuilder.BuildForToolPlanning(conversation, activeTodosSnapshot.TodoIds);

    // Phase 1: Tool Planning (JSON output)
    var toolPlanResult = await (await apolloAIAgent
      .CreateToolPlanningRequestAsync(toolPlanningMessages, userTimezone, activeTodosSnapshot.Summary))
      .ExecuteAsync(cancellationToken);

    var toolPlan = toolPlanResult switch
    {
      { Success: true, Content: var content } => ProcessToolPlanParsing(person.Id.Value, content),
      { Success: false, ErrorMessage: var errorMsg } => LogToolPlanningFailure(person.Id.Value, errorMsg),
      _ => new ToolPlan()
    };

    // Phase 2: Validate + Execute Tool Calls
    var validationContext = new ToolPlanValidationContext(
      plugins,
      toolPlanningMessages,
      activeTodosSnapshot.TodoIds);

    var validationResult = ToolPlanValidator.Validate(toolPlan, validationContext);
    var toolResults = new List<ToolCallResult>(validationResult.BlockedCalls);

    if (validationResult.ApprovedCalls.Count > 0)
    {
      var executed = await ToolExecutionService.ExecuteToolPlanAsync(validationResult.ApprovedCalls, plugins, cancellationToken);
      toolResults.AddRange(executed);
    }

    ConversationLogs.ToolExecutionCompleted(
      logger,
      person.Id.Value,
      validationResult.ApprovedCalls.Count,
      validationResult.BlockedCalls.Count,
      toolResults.Count - validationResult.BlockedCalls.Count);

    // Phase 3: Response Generation
    var actionsSummary = toolResults.Count == 0
      ? "None"
      : string.Join("\n", toolResults.Select(tc => $"- {tc.ToSummary()}"));

    ConversationLogs.ActionsTaken(logger, person.Id.Value, [actionsSummary]);

    var responseResult = await (await apolloAIAgent
      .CreateResponseRequestAsync(responseMessages, actionsSummary, userTimezone))
      .ExecuteAsync(cancellationToken);

    return !responseResult.Success
      ? $"I encountered an issue while processing your request: {responseResult.ErrorMessage}"
      : responseResult.Content;
  }

  private Dictionary<string, object> CreatePlugins(Person person)
  {
    var toDoPlugin = new ToDoPlugin(mediator, personStore, timeParsingService, personConfig, person.Id);
    var remindersPlugin = new RemindersPlugin(mediator, personStore, timeParsingService, personConfig, person.Id);
    var personPlugin = new PersonPlugin(personStore, personConfig, person.Id);

    return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
    {
      [ToDoPlugin.PluginName] = toDoPlugin,
      [RemindersPlugin.PluginName] = remindersPlugin,
      [PersonPlugin.PluginName] = personPlugin
    };
  }

  private async Task SaveReplyAsync(Conversation conversation, string response, CancellationToken cancellationToken)
  {
    var addReplyResult = await conversationStore.AddReplyAsync(conversation.Id, new Content(response), cancellationToken);

    if (addReplyResult.IsFailed)
    {
      DataAccessLogs.UnableToSaveMessageToConversation(logger, conversation.Id.Value, response);
    }
  }

  private Result<Reply> CreateReplyToUser(string response)
  {
    var currentTime = timeProvider.GetUtcDateTime();
    return Result.Ok(new Reply
    {
      Content = new(response),
      CreatedOn = new(currentTime),
      UpdatedOn = new(currentTime)
    });
  }

  private static string GetUserTimezone(Person person)
  {
    return person.TimeZoneId?.Value ?? "UTC";
  }

  private async Task<ActiveTodosSnapshot> BuildActiveTodosSnapshotAsync(PersonId personId, CancellationToken cancellationToken)
  {
    var todosResult = await toDoStore.GetByPersonIdAsync(personId, includeCompleted: false, cancellationToken);

    if (todosResult.IsFailed || !todosResult.Value.Any())
    {
      return new ActiveTodosSnapshot("No active todos", []);
    }

    var todos = todosResult.Value
      .OrderBy(t => t.DueDate?.Value)
      .Take(10) // Limit to 10 to avoid bloating the prompt
      .ToList();

    var summary = string.Join("\n", todos.Select(t =>
    {
      var dueDate = t.DueDate?.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "No due date";
      return $"• [{t.Id.Value}] {t.Description.Value} (Due: {dueDate})";
    }));

    var todoIds = todos.ConvertAll(t => t.Id.Value.ToString());
    return new ActiveTodosSnapshot(summary, todoIds);
  }

  private sealed record ActiveTodosSnapshot(string Summary, IReadOnlyCollection<string> TodoIds);

  private ToolPlan ProcessToolPlanParsing(Guid personId, string content)
  {
    var parseResult = ToolPlanParser.Parse(content);
    return parseResult switch
    {
      { IsSuccess: true, Value: var toolPlan } => LogAndReturnToolPlan(personId, toolPlan),
      _ => LogParsingFailure(personId, parseResult)
    };
  }

  private ToolPlan LogAndReturnToolPlan(Guid personId, ToolPlan toolPlan)
  {
    ConversationLogs.ToolPlanReceived(logger, personId, toolPlan.ToolCalls.Count);
    return toolPlan;
  }

  private ToolPlan LogParsingFailure(Guid personId, Result<ToolPlan> parseResult)
  {
    var errorMsg = parseResult.Errors.Count > 0 ? parseResult.Errors[0].Message : "Unknown error";
    ConversationLogs.ToolPlanParsingFailed(logger, personId, errorMsg);
    return new ToolPlan();
  }

  private ToolPlan LogToolPlanningFailure(Guid personId, string errorMsg)
  {
    ConversationLogs.ToolPlanningRequestFailed(logger, personId, errorMsg);
    return new ToolPlan();
  }
}
