using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    class TimerTrigger1
    {
        int count = 0;

        [FunctionName("TimerTrigger1")]
        [Timeout("00:00:06")]
        public async Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, CancellationToken cancellationToken)
        {
            //log.LogInformation($"TimerTrigger1 start --- {++count} [{DateTime.UtcNow:u}]");
            Console.WriteLine($"TimerTrigger1 start --- {++count} [{DateTime.UtcNow:u}]");

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            //log.LogInformation($"TimerTrigger1 end [{DateTime.UtcNow:u}]");
            Console.WriteLine($"TimerTrigger1 end [{DateTime.UtcNow:u}]");
            
            //throw new InvalidOperationException("TimerTrigger1 exception");
            //return "TimerTrigger1";
        }
    }
}
