using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    class ServiceBusTrigger1
    {
        [FunctionName("ServiceBusTrigger1")]
        [Timeout("00:00:10")]
        public async Task RunAsync([ServiceBusTrigger("queue1", Connection = "ServiceBusConnection1")] string myQueueItem, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation($"[{myQueueItem}] received");
            log.LogWarning($"[{myQueueItem}] warning");
            await Task.Delay(5000);
            log.LogError($"[{myQueueItem}] error");
            log.LogCritical($"[{myQueueItem}] critial");

            //if (myQueueItem.StartsWith("Message 2"))
            //{
            //    throw new System.Exception("Testing Exception");
            //}
        }
    }
}
