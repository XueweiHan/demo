using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace azfunc
{
    public class TimerTrigger1
    {
        [FunctionName("TimerTrigger1")]
        public async Task Run([TimerTrigger("*/3 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"TimerTrigger1 start {{");
            await Task.Delay(2000);
            log.LogInformation($"TimerTrigger1 end   }}");
        }
    }
}
