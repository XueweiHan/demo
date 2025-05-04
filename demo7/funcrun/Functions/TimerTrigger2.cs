using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Functions
{
    public class TimerTrigger2
    {
        [FunctionName("TimerTrigger2")]
        public static void Run([TimerTrigger("*/2 * * * * *", RunOnStartup = true)]  ILogger log)
        {
            //throw new System.Exception("TimerTrigger2 exception");
            log.LogWarning($"TimerTrigger2 start     >>");
            Thread.Sleep(1000);
            log.LogWarning($"TimerTrigger2 end       <<");
        }
    }
}
