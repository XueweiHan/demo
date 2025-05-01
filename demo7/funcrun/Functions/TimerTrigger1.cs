using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    public class TimerTrigger1
    {
        [FunctionName("TimerTrigger1")]
        [Timeout("00:00:03")]
        public async Task RunAsync([TimerTrigger("*/7 * * * * *")] TimerInfo myTimer, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation($"TimerTrigger1 start [{DateTime.UtcNow:u}]");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            log.LogInformation($"TimerTrigger1 end [{DateTime.UtcNow:u}]");

            throw new System.Exception("TimerTrigger1 exception");
        }
    }
}
