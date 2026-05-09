using System.ComponentModel;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

[Description("Musteri profili ve siparis gecmisi bilgilerini getiren plugin.")]
public class CustomerPlugin
{
    private readonly ICustomerService _customerService;

    public CustomerPlugin(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [KernelFunction("GetCustomerProfile")]
    [Description("READ_ONLY. Verilen musteri ID'si icin profil bilgilerini getirir. Mock veri ornekleri: cust-001 Ahmet Yilmaz, cust-002 Ayse Demir, cust-003 Mehmet Kaya.")]
    public string GetCustomerProfile(
        [Description("Profili getirilecek musteri ID'si. Ornek: cust-001.")] string customerId)
    {
        var result = _customerService.GetCustomerProfile(customerId);
        return PluginResponseSerializer.Serialize(result);
    }

    [KernelFunction("GetOrderHistory")]
    [Description("READ_ONLY. Verilen musteri ID'si icin siparis gecmisini en yeniden eskiye dogru getirir.")]
    public string GetOrderHistory(
        [Description("Siparis gecmisi getirilecek musteri ID'si. Ornek: cust-001.")] string customerId)
    {
        var result = _customerService.GetOrderHistory(customerId);
        return PluginResponseSerializer.Serialize(result);
    }
}
