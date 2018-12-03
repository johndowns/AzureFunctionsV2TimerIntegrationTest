using System;
using System.Threading.Tasks;
using FunctionApp.IntegrationTest.Collections;
using FunctionApp.IntegrationTest.Fixtures;
using FunctionApp.IntegrationTest.Settings;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using TestStack.BDDfy;
using Xunit;

namespace FunctionApp.IntegrationTest.Tests
{
    [Collection(nameof(FunctionTestCollection))]
    public class HelloTimerFunctionTest
    {
        private readonly FunctionTestFixture _fixture;

        private readonly CloudQueue _queue = CloudStorageAccount
            .Parse(ConfigurationHelper.Settings.StorageConnectionString)
            .CreateCloudQueueClient()
            .GetQueueReference("greetings");

        private CloudQueueMessage _message;
        
        public HelloTimerFunctionTest(FunctionTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Hello_Timer_Function_Test()
        {
            this.BDDfy();
        }

        private async Task Given_The_Greetings_Queue_Exists_And_Is_Empty()
        {
            await _queue.CreateIfNotExistsAsync();
            await _queue.ClearAsync();
        }
        
        private Task When_The_HelloTimer_Function_Is_Invoked()
        {
            return _fixture.InvokeTimerTriggeredFunction("HelloTimer");
        }

        private async Task When_We_Wait_For_A_Queue_Message_To_Be_Enqueued()
        {
            // try getting the queue message every 2 seconds for 20 seconds
            // this allows for some delay in the processing of the request
            var timeoutTime = DateTime.UtcNow.AddSeconds(20);
            while (DateTime.UtcNow < timeoutTime)
            {
                _message = await _queue.GetMessageAsync();
                if (_message != null)
                {
                    break;
                }

                await Task.Delay(2000);
            }
        }

        private void Then_The_Enqueued_Message_Contents_Should_Be_James_Bond()
        {
            Assert.EndsWith("James Bond", _message.AsString);
        }

        public Task Setup()
        {
            return _fixture.WaitForAppToRespond();
        }

        public Task TearDown()
        {
            return _queue.ClearAsync();
        }
    }
}
