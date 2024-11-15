using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace azfunc
{
    public class TimerTrigger2
    {
        [FunctionName("TimerTrigger2")]
        [Timeout("00:00:10")]
        public void Run([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, ILogger log, CancellationToken cancellationToken)
        {
            log.LogWarning($"TimerTrigger2 start     {{");
            Thread.Sleep(3000);
            log.LogWarning($"TimerTrigger2 end       }}");
        }
    }
}
