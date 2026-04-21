using Microsoft.SemanticKernel;

namespace ECommerceAgent.ConsoleApp.Filters;

public class ToolLoggingFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        // TODO: Pair programming ile implemente edilecek
        // - Tool adı logla
        // - Parametreleri logla
        // - Süreyi ölç
        // - Sonucu logla
        throw new NotImplementedException();
    }
}
