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

        private async Task ModifyFile(string fileName)
        {
            using (var writer = new StreamWriter(fileName, true))
            {
                await writer.WriteAsync("More of my stuff");
                await writer.FlushAsync();
            }
        }

        private void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        [Fact]
        public async Task ShouldExecuteDelegateOnFileCreation()
        {
            var callReceived = false;
            var filename = "create.xxx";
            DeleteFile(filename);
            var poller = new FilePoller(".\\", (f) =>
            {
                if (
                    f.Name == filename)
                {
                    callReceived = true;
                }
            });

            poller.StartPolling(10);
            await CreateFile(filename);

            await Task.Delay(100);
            Assert.True(callReceived);
        }

        [Fact]
        public async Task ShouldExecuteDelegateOnFileModification()
        {
            var callReceived = false;
            var filename = "Modify.xxx";
            DeleteFile(filename);
            await CreateFile(filename);
            var poller = new FilePoller(".\\", null, (f) =>
            {
                if (f.Name == filename)
                {
                    callReceived = true;
                }
            });

            poller.StartPolling(10);
            await Task.Delay(20);
            await ModifyFile(filename);

            await Task.Delay(30);
            Assert.True(callReceived);
        }

        [Fact]
        public async Task ShouldExecuteDelegateOnFileDeletion()
        {
            var callReceived = false;
            var filename = "Delete.xxx";
            DeleteFile(filename);
            await CreateFile(filename);
            var poller = new FilePoller(".\\", null, null, (f) =>
            {
                if (f.Name == filename)
                {
                    callReceived = true;
                }
            });

            poller.StartPolling(10);
            DeleteFile(filename);

            await Task.Delay(30);
            Assert.True(callReceived);
        }

        [Fact]
        public async Task ShouldCallOnErrorIfErrorOccurs()
        {
            var callReceived = false;
            var filename = "Delete.xxx";
            DeleteFile(filename);
            await CreateFile(filename);

            var poller = new FilePoller(".\\", null, null,  (file) => 
                throw new Exception(), null,
                (filenameFail, e) => { callReceived = true; });

            poller.StartPolling(10);
            File.Delete(filename);

            await Task.Delay(30);
            Assert.True(callReceived);
        }
        
        [Fact]
        public async Task ShouldCallOnSuccess()
        {
            var callReceived = false;
            var filename = "Delete.xxx";
            DeleteFile(filename);

            var poller = new FilePoller(".\\", null, null, null,  (fileInfo) => { callReceived = true; });

            poller.StartPolling(10);
            File.Delete(filename);

            await Task.Delay(30);
            Assert.True(callReceived);
        }
    }
}