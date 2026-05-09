namespace ECommerceAgent.ConsoleApp.Guardrails;

public sealed class InputGuardrail
{
    private static readonly string[] PromptInjectionSignals =
    [
        "ignore previous",
        "ignore all previous",
        "forget previous",
        "system prompt",
        "developer message",
        "show your instructions",
        "reveal your instructions",
        "talimatlari unut",
        "onceki talimatlari unut",
        "sistem prompt",
        "sistem mesajini goster",
        "developer mesajini goster",
        "kurallari yok say",
        "talimatlari yok say"
    ];

    private static readonly string[] OutOfScopeWriteSignals =
    [
        "bedava yap",
        "ucretsiz yap",
        "fiyati sifirla",
        "tum urunleri sil",
        "stoklari sifirla",
        "stoklari degistir",
        "api key",
        "secret key"
    ];

    public GuardrailResult Validate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return GuardrailResult.Block(
                "EMPTY_INPUT",
                "Bos istek islenemez.");
        }

        var normalized = Normalize(input);

        if (PromptInjectionSignals.Any(normalized.Contains))
        {
            return GuardrailResult.Block(
                "PROMPT_INJECTION_SUSPECTED",
                "Bu istegi guvenlik nedeniyle isleyemiyorum.");
        }

        if (OutOfScopeWriteSignals.Any(normalized.Contains))
        {
            return GuardrailResult.Block(
                "OUT_OF_SCOPE_WRITE_REQUEST",
                "Bu islem yetkim disinda. Urun arama, sepet, musteri ve siparis islemlerinde yardimci olabilirim.");
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
