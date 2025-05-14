using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionsApp2
{
    public class Functions1
    {
        [FunctionName("Function1")]
        public void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }

    public class Function2
    {
        [FunctionName("Function2")]
        public void Run([ServiceBusTrigger("queue2", Connection = "MyServiceBusConnection")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        }
    }
}
