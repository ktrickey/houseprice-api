using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.Extensions.Configuration;
using RestSharp;
using Serilog;

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

            var apiEndpoint = configuration["endPoint"];

            var client = new RestClient(apiEndpoint);


            //Monitor the drop directory for new files - Docker will map to file system as will k8s
            // Filewatcher not reliable, especially in Linux Docker containers on Windows, therefore
            // we'll have one going, but back it up with

//            var watchDirectory = "/transaction_data/Import/Drop";
//            var successDirectory = "/transaction_data/Import/Complete";

            var watchDirectory = configuration["watchDirectory"];
            var successDirectory = configuration["successDirectory"];

            var watcher = new PollingWatcher( new FilePoller(watchDirectory, async (f) =>
            {
                Log.Information($"Processing {f.Name}");
                using (var streamReader = new StreamReader( new FileStream(f.FullName, FileMode.Open, FileAccess.Read)))
                {
                    // change this to push to the transaction API
                    var req = new RestRequest($"api/transaction/{f.Name}");
                    req.AddBody(new {transactions = await streamReader.ReadToEndAsync()});
                    req.RequestFormat = DataFormat.Json;
                    var result = await client.ExecutePostTaskAsync<object>(req);

                }

                File.Move(f.FullName, Path.Combine(successDirectory, f.Name));
            }));

            watcher.StartPolling();

            while (true)
            {
                Thread.Sleep(5000);
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}