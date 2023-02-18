using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    public class ReserveOrderItems
    {
        [FunctionName("ReserveOrderItems")]
        public async Task Run([ServiceBusTrigger("orderitems", Connection = "sbConnection")] string message,
            [Blob("reservations/{rand-guid}.json", FileAccess.Write, Connection = "bsConnection")] TextWriter blobWriter,
            ILogger log)
        {
            log.LogInformation($"Received a message: {message}");

            await blobWriter.WriteAsync(message);
        }
    }
}
