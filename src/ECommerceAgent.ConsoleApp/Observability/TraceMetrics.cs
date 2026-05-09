namespace ECommerceAgent.ConsoleApp.Observability;

public sealed class TraceMetrics
{
    public long DurationMs { get; set; }
    public int ToolCallCount { get; set; }
    public int ErrorCount { get; set; }
    public int EscalationCount { get; set; }
    public long TotalToolDurationMs { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}
