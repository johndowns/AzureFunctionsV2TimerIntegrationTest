using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionApp.IntegrationTest.Settings;

namespace FunctionApp.IntegrationTest.Fixtures
{
    public class FunctionTestFixture : IDisposable
    {
        private readonly Process _funcHostProcess;

        public int Port { get; } = 7001;

        public readonly HttpClient Client = new HttpClient();

        public FunctionTestFixture()
        {
            var dotnetExePath = Environment.ExpandEnvironmentVariables(ConfigurationHelper.Settings.DotnetExecutablePath);
            var functionHostPath = Environment.ExpandEnvironmentVariables(ConfigurationHelper.Settings.FunctionHostPath);
            var functionAppFolder = Path.GetRelativePath(Directory.GetCurrentDirectory(), ConfigurationHelper.Settings.FunctionApplicationPath);

            _funcHostProcess = new Process
            {
                StartInfo =
                {
                    FileName = dotnetExePath,
                    Arguments = $"\"{functionHostPath}\" start -p {Port}",
                    WorkingDirectory = functionAppFolder
                }
            };
            var success = _funcHostProcess.Start();
            if (!success)
            {
                throw new InvalidOperationException("Could not start Azure Functions host.");
            }

            Client.BaseAddress = new Uri($"http://localhost:{Port}");
        }

        public async Task WaitForAppToRespond()
        {
            var timeoutTime = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < timeoutTime)
            {
                var response = await Client.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                await Task.Delay(1000);
            }
        }

        public async Task InvokeTimerTriggeredFunction(string functionName)
        {
            var content = new StringContent("{\"input\":\"\"}", Encoding.Default, "application/json");
            var response = await Client.PostAsync($"admin/functions/{functionName}", content);
            response.EnsureSuccessStatusCode();
        }

        public virtual void Dispose()
        {
            if (!_funcHostProcess.HasExited)
            {
                _funcHostProcess.Kill();
            }

            _funcHostProcess.Dispose();
        }
    }
}
