using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;
public class ServiceBusOrderItemsReserver : IOrderItemsReserver
{
    private readonly OrderItemsReservingSettings _settings;

    public ServiceBusOrderItemsReserver(OrderItemsReservingSettings settings)
    {
        _settings = settings;
    }

    public async Task ReserveAsync(IEnumerable<OrderItem> orderItems)
    {
        await using var client = new ServiceBusClient(_settings.OrderItemsServiceBusConnection);
        await using ServiceBusSender? sender = client.CreateSender(_settings.OrderItemsQueueName);

        string messageBody = orderItems
            .GroupBy(x => x.ItemOrdered.CatalogItemId, x => x.Units)
            .Select(x => new { ItemId = x.Key, Quantity = x.Sum() })
            .ToJson();

        await sender.SendMessageAsync(new ServiceBusMessage(messageBody));
    }
}
