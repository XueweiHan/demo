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
            log.LogInformation($"C# ServiceBus queue trigger function processed message: [{myQueueItem}]");
            //System.Threading.Thread.Sleep(1000);
            log.LogError("Testing Error Log");
            log.LogWarning("Testing Warning Log");
            log.LogCritical("Testing Critical Log");
            log.LogDebug("Testing Debug Log");
            log.LogTrace("Testing Trace log");

            //if (myQueueItem.StartsWith("Message 2"))
            //{
            //    throw new System.Exception("Testing Exception");
            //}
        }
    }
}
