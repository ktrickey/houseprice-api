using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace HousePrice.Api.ImportFileWatcher.Tests
{
    public class FilePollerTests
    {
        private async Task CreateFile(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                await writer.WriteAsync("My stuff");
            }
        }
        [Fact]
        public async Task ShouldExecuteDelegateOnFileCreation()
        {
            var callReceived = false;
            var filename = "xxxx.xxx";
            var poller = new FilePoller(".\\", (f) => { if (
                f.Name ==  filename)
                { callReceived= true;} });

             poller.StartPolling(1000);
             await CreateFile("xxxx.xxx");

            await Task.Delay(2000);
            Assert.True(callReceived);
        }
    }
}