namespace ECommerceAgent.ConsoleApp.Observability;

public sealed class AgentTraceContext
{
    private readonly AsyncLocal<AgentTrace?> _currentTrace = new();

    public AgentTrace? Current => _currentTrace.Value;

    public AgentTrace BeginTurn(string conversationId, string userInput)
    {
        var trace = new AgentTrace
        {
            ConversationId = conversationId,
            TurnId = Guid.NewGuid().ToString("N"),
            StartedAtUtc = DateTime.UtcNow,
            UserInput = userInput
        };

        _currentTrace.Value = trace;
        return trace;
    }

    public void AddToolCall(ToolCallTrace toolCall)
    {
        var trace = Current;
        if (trace == null)
            return;

        trace.ToolCalls.Add(toolCall);
        trace.Metrics.ToolCallCount = trace.ToolCalls.Count;
        trace.Metrics.TotalToolDurationMs = trace.ToolCalls.Sum(call => call.DurationMs);
        trace.Metrics.ErrorCount = trace.ToolCalls.Count(call => !call.Success || call.ExceptionType != null);
        trace.Metrics.EscalationCount = trace.ToolCalls.Count(call => call.RequiresEscalation);
        trace.Metrics.ReadOnlyToolCallCount = trace.ToolCalls.Count(call => call.RiskLevel == "ReadOnly");
        trace.Metrics.WriteToolCallCount = trace.ToolCalls.Count(call =>
            call.RiskLevel == "LowRiskWrite" ||
            call.RiskLevel == "MediumRiskWrite");
    }

    public void MarkInputBlocked(string reasonCode, string message)
    {
        var trace = Current;
        if (trace == null)
            return;

        trace.BlockedByInputGuardrail = true;
        trace.InputGuardrailReasonCode = reasonCode;
        trace.InputGuardrailMessage = message;
        trace.Metrics.GuardrailBlockCount++;
    }

    public void MarkOutputWarning(string reasonCode, string message)
    {
        var trace = Current;
        if (trace == null)
            return;

        trace.OutputGuardrailWarning = true;
        trace.OutputGuardrailReasonCode = reasonCode;
        trace.OutputGuardrailMessage = message;
        trace.Metrics.GuardrailWarningCount++;
    }

    public void CompleteTurn(string assistantOutput, long durationMs)
    {
        var trace = Current;
        if (trace == null)
            return;

        trace.AssistantOutput = assistantOutput;
        trace.CompletedAtUtc = DateTime.UtcNow;
        trace.Metrics.DurationMs = durationMs;
    }

    public void FailTurn(Exception exception, long durationMs)
    {
        var trace = Current;
        if (trace == null)
            return;

        trace.FailureMessage = $"{exception.GetType().Name}: {exception.Message}";
        trace.CompletedAtUtc = DateTime.UtcNow;
        trace.Metrics.DurationMs = durationMs;
        trace.Metrics.ErrorCount++;
    }

    public void Clear()
    {
        _currentTrace.Value = null;
    }
}
