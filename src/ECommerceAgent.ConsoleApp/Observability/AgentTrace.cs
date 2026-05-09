namespace ECommerceAgent.ConsoleApp.Observability;

public sealed class AgentTrace
{
    public string ConversationId { get; init; } = string.Empty;
    public string TurnId { get; init; } = string.Empty;
    public DateTime StartedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; set; }
    public string UserInput { get; init; } = string.Empty;
    public string? AssistantOutput { get; set; }
    public string? FailureMessage { get; set; }
    public List<ToolCallTrace> ToolCalls { get; } = new();
    public TraceMetrics Metrics { get; } = new();
}
