namespace ECommerceAgent.ConsoleApp.Observability;

public sealed class ToolCallTrace
{
    public string PluginName { get; init; } = string.Empty;
    public string FunctionName { get; init; } = string.Empty;
    public string RiskLevel { get; init; } = string.Empty;
    public Dictionary<string, string?> Arguments { get; init; } = new();
    public long DurationMs { get; init; }
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public bool RequiresEscalation { get; init; }
    public string? Message { get; init; }
    public string ResultPreview { get; init; } = string.Empty;
    public string? ExceptionType { get; init; }
    public string? ExceptionMessage { get; init; }
}
