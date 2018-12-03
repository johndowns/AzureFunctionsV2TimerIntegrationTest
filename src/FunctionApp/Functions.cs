using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class Functions
    {
        [FunctionName("HelloTimer")]
        [return: Queue("greetings")]
        public static string HelloQueue(
            [TimerTrigger(scheduleExpression: "0 30 9 * * 1")] TimerInfo timerInfo,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            return $"Hello, James Bond";
        }
    }
}
