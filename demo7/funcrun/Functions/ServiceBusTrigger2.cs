using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class ServiceBusTrigger2
    {
        [FunctionName("ServiceBusTrigger2")]
        public void Run([ServiceBusTrigger("queue2", Connection = "ServiceBusConnection2")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger [queue2]: {myQueueItem}");
        }
    }
}
