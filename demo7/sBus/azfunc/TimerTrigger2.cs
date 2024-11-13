using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace azfunc
{
    public class TimerTrigger2
    {
        [FunctionName("TimerTrigger2")]
        public void Run([TimerTrigger("0/5 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogWarning($"TimerTrigger2 start");
            Thread.Sleep(3000);
            log.LogWarning($"TimerTrigger2 end");
        }
    }
}
