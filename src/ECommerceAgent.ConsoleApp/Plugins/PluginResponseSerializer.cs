using System.Text.Json;
using ECommerceAgent.Application.Common;

namespace ECommerceAgent.ConsoleApp.Plugins;

internal static class PluginResponseSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Serialize<T>(Result<T> result)
    {
        return JsonSerializer.Serialize(result, Options);
    }
}
