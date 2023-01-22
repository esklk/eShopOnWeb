using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Web.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace DeliveryService
{
    public static class Orders
    {
        [FunctionName("orders")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest req,
            [CosmosDB(databaseName: "delivery", containerName: "orders", Connection = "CosmosDbConnectionString")]
            IAsyncCollector<dynamic> orders)
        {
            string body = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return new BadRequestErrorMessageResult("Body cannot be empty.");
            }

            var order = JsonConvert.DeserializeObject<DeliveryOrder>(body);
            if (order == null)
            {
                return new BadRequestErrorMessageResult("Body must be a json representation of a delivery order.");
            }

            if (order.ShippingAddress == null)
            {
                return new BadRequestErrorMessageResult("Shipping address is required.");
            }

            if (!order.OrderItems.Any())
            {
                return new BadRequestErrorMessageResult("Order must consist of at least 1 order item.");
            }

            await orders.AddAsync(new
            {
                id = Guid.NewGuid().ToString(),
                shippingAddress = order.ShippingAddress,
                items = order.OrderItems.Select(x => new { id = x.ItemOrdered.CatalogItemId, units = x.Units, unitPrice = x.UnitPrice }),
                totalPrice = order.OrderItems.Sum(x => x.Units * x.UnitPrice)
            });

            return new OkResult();
        }
    }
}
