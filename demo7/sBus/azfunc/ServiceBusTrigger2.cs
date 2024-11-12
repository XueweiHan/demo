using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace azfunc
{
    public class ServiceBusTrigger2
    {
        [FunctionName("ServiceBusTrigger2")]
        public void Run([ServiceBusTrigger("demo7queue2", Connection = "ServiceBusConnection2")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger [demo7queue2]: {myQueueItem}");
        }
    }
}
