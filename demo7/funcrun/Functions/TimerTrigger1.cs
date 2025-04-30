using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Functions
{
    public class TimerTrigger1
    {
        [FunctionName("TimerTrigger1")]
        [Timeout("00:00:10")]
        public async Task Run([TimerTrigger("*/3 * * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log, CancellationToken cancellationToken)
        {
            log.LogInformation($"TimerTrigger1 start {{");
            await Task.Delay(2000);
            log.LogInformation($"TimerTrigger1 end   }}");
        }
    }
}
