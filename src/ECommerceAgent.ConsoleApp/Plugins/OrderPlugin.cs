using System.ComponentModel;
using Microsoft.SemanticKernel;
using ECommerceAgent.Application.Interfaces;

namespace ECommerceAgent.ConsoleApp.Plugins;

[Description("Siparis durumu, siparis detayi ve siparis iptali islemlerini yoneten plugin.")]
public class OrderPlugin
{
    private readonly IOrderService _orderService;

    public OrderPlugin(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [KernelFunction("GetOrderStatus")]
    [Description("READ_ONLY. Verilen siparis ID'si icin siparis durumunu getirir. Siparis ID formatina ornek: ord-1002.")]
    public string GetOrderStatus(
        [Description("Durumu sorgulanacak siparis ID'si. Ornek: ord-1002.")] string orderId)
    {
        var result = _orderService.GetOrderStatus(orderId);
        return PluginResponseSerializer.Serialize(result);
    }

    [KernelFunction("GetOrderDetails")]
    [Description("READ_ONLY. Verilen siparis ID'si icin urunler, toplam tutar, durum ve iptal bilgileri dahil detayli siparis bilgisini getirir.")]
    public string GetOrderDetails(
        [Description("Detaylari getirilecek siparis ID'si. Ornek: ord-1001.")] string orderId)
    {
        var result = _orderService.GetOrderDetails(orderId);
        return PluginResponseSerializer.Serialize(result);
    }

    [KernelFunction("CancelOrder")]
    [Description("WRITE_ACTION. Verilen siparisi iptal etmeyi dener. Sadece is kurallari izin verirse iptal eder; teslim edilmis veya teslimata cikmis siparislerde eskalasyon bilgisi doner.")]
    public string CancelOrder(
        [Description("Iptal edilecek siparis ID'si. Ornek: ord-1002.")] string orderId,
        [Description("Kullanicinin belirttigi net iptal nedeni. Bos birakilmamalidir.")] string reason)
    {
        var result = _orderService.CancelOrder(orderId, reason);
        return PluginResponseSerializer.Serialize(result);
    }
}
