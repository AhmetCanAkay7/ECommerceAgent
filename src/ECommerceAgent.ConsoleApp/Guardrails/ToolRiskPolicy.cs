namespace ECommerceAgent.ConsoleApp.Guardrails;

public sealed class ToolRiskPolicy
{
    public ToolRiskLevel Classify(string functionName)
    {
        return functionName switch
        {
            "SearchProducts" => ToolRiskLevel.ReadOnly,
            "GetCart" => ToolRiskLevel.ReadOnly,
            "GetCustomerProfile" => ToolRiskLevel.ReadOnly,
            "GetOrderHistory" => ToolRiskLevel.ReadOnly,
            "GetOrderStatus" => ToolRiskLevel.ReadOnly,
            "GetOrderDetails" => ToolRiskLevel.ReadOnly,
            "AddToCart" => ToolRiskLevel.LowRiskWrite,
            "RemoveFromCart" => ToolRiskLevel.LowRiskWrite,
            "CancelOrder" => ToolRiskLevel.MediumRiskWrite,
            _ => ToolRiskLevel.Unknown
        };
    }
}
