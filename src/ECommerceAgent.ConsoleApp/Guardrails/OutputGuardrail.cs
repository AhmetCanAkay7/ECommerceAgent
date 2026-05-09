using ECommerceAgent.ConsoleApp.Observability;

namespace ECommerceAgent.ConsoleApp.Guardrails;

public sealed class OutputGuardrail
{
    private static readonly string[] StrongCompletionPhrases =
    [
        "iptal edildi",
        "islem tamamlandi",
        "basariyla tamamlandi",
        "tamamladim"
    ];

    public GuardrailResult Validate(string assistantOutput, AgentTrace trace)
    {
        if (trace.Metrics.EscalationCount == 0)
            return GuardrailResult.Allow();

        var normalized = Normalize(assistantOutput);
        if (StrongCompletionPhrases.Any(normalized.Contains))
        {
            return GuardrailResult.Block(
                "ESCALATION_OUTPUT_RISK",
                "Cevap escalation gereken bir islemi tamamlanmis gibi ifade ediyor olabilir.");
        }

        return GuardrailResult.Allow();
    }

    private static string Normalize(string input)
    {
        return input
            .Trim()
            .ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }
}
