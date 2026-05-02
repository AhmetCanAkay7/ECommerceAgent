namespace ECommerceAgent.Domain.Entities;

// Siparis durumu gibi kritik state'leri serbest metne birakmiyoruz.
public enum OrderStatus
{
    Preparing,
    OutForDelivery,
    Delivered,
    Cancelled
}
