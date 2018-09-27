using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.Extensions.Configuration;

namespace HousePrice.Api.ImportFileWatcher
{
    class Program
    {
        static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            var configuration = builder.Build();


            //Monitor the drop directory for new files - Docker will map to file system as will k8s
            // Filewatcher not reliable, especially in Linux Docker containers on Windows, therefore
            // we'll have one going, but back it up with
            var watchDirectory = "/transaction_data/Import/Drop";
            var successDirectory = "/transaction_data/Import/Complete";
            var files = Directory.GetFiles(watchDirectory);

            PostcodeLookup.GetByPostcode("");
            var importer = new Importer();

            var watcher = new PollingWatcher(new FilePoller(watchDirectory, (f) =>
            {
                Console.WriteLine($"Processing {f.Name}");
                using (var fileStream = new FileStream(f.FullName, FileMode.Open, FileAccess.Read))
                {
                    var task = importer.Import(f.Name, fileStream);
                    task.Wait();
                }

                File.Move(f.FullName, Path.Combine(successDirectory, f.Name));
            }));

            watcher.StartPolling();

            while (1 == 2)
            {
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}