using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace azfunc
{
    public class ServiceBusTrigger
    {
        [FunctionName("ServiceBusTrigger")]
        public async Task RunAsync([ServiceBusTrigger("demo7queue", Connection = "ServiceBusConnection")] string myQueueItem, ILogger log)
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
