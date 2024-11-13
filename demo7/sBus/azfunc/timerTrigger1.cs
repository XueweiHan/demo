using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace azfunc
{
    public class timerTrigger1
    {
        [FunctionName("timerTrigger1")]
        public void Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
