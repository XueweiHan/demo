using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Functions
{
    public class TimerTrigger2
    {
        [FunctionName("TimerTrigger2")]
        public void Run([TimerTrigger("*/11 * * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            //throw new System.Exception("TimerTrigger2 exception");
            log.LogWarning($"TimerTrigger2 start     >>");
            Thread.Sleep(3000);
            log.LogWarning($"TimerTrigger2 end       <<");
        }
    }
}
