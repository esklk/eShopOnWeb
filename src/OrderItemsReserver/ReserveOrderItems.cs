using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrderItemsReserver
{
    public class ReserveOrderItems
    {
        private const int RetryAttempts = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

        [FunctionName("ReserveOrderItems")]
        public async Task Run([ServiceBusTrigger("orderitems", Connection = "sbConnection")] string message,
            [Blob("reservations/{rand-guid}.json", FileAccess.Write, Connection = "bsConnection")] TextWriter blobWriter,
            ILogger log)
        {
            log.LogInformation($"Received a message: {message}");

            int attempt = 1;
            do
            {
                try
                {
                    await blobWriter.WriteAsync(message);
                    break;
                }
                catch (Exception)
                {
                    if (attempt == RetryAttempts)
                    {
                        throw;
                    }

                    attempt++;
                    Thread.Sleep(RetryDelay);
                    log.LogFunctionRetryAttempt(RetryDelay,attempt, 3);
                }
            } while (true);
        }
    }
}
