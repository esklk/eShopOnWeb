using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class Reserve
    {
        [FunctionName(nameof(Reserve))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [Blob("orders/{rand-guid}.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] TextWriter blobWriter)
        {
            string body = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                return new BadRequestErrorMessageResult("Body cannot be empty.");
            }

            OrderItem[] orderItems = JsonConvert.DeserializeObject<OrderItem[]>(body);
            if (orderItems == null)
            {
                return new BadRequestErrorMessageResult("Body must be a json representation of a collection of order items.");
            }

            var itemsReservation = orderItems
                .GroupBy(x => x.ItemOrdered.CatalogItemId)
                .Select(x => new
                {
                    ItemId = x.Key,
                    Quantity = x.Sum(y => y.Units)
                });
            await blobWriter.WriteAsync(JsonConvert.SerializeObject(itemsReservation));

            return new OkResult();
        }
    }
}
