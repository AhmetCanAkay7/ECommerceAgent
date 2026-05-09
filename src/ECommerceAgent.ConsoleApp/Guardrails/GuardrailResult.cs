namespace ECommerceAgent.ConsoleApp.Guardrails;

public sealed class GuardrailResult
{
    public bool Allowed { get; init; }
    public string? ReasonCode { get; init; }
    public string Message { get; init; } = string.Empty;

    public static GuardrailResult Allow()
    {
        return new GuardrailResult
        {
            Allowed = true,
            Message = "Allowed"
        };
    }

    public static GuardrailResult Block(string reasonCode, string message)
    {
        return new GuardrailResult
        {
            Allowed = false,
            ReasonCode = reasonCode,
            Message = message
        };
    }
}
