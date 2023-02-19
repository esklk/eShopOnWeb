namespace Microsoft.eShopWeb.ApplicationCore;
public class OrderItemsReservingSettings
{
    public string? OrderItemsServiceBusConnection { get; set; }
    public string? OrderItemsQueueName { get; set; }
}
