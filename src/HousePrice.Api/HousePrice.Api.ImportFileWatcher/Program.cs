using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using HousePrice.Api.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using Serilog.Events;

namespace HousePrice.Api.ImportFileWatcher
{
    class Program
    {
        static async Task Main()
        {
	        Log.Logger = new LoggerConfiguration()
		        .MinimumLevel.Debug()
		        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
		        .Enrich.FromLogContext()
		        .WriteTo.Console()
		        .CreateLogger();


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
			var processingDirectory = configuration["processingDirectory"];

            var watcher = new PollingWatcher( new FilePoller(watchDirectory, async (f) =>
            {
                Log.Information($"Processing {f.FullName}");
	            var filename = $"{f.Name}-{Guid.NewGuid().ToString()}";
	            var processingFile = Path.Combine(processingDirectory, filename);
	
	            File.Move(f.FullName, processingFile);

				using (var stream =  new FileStream(processingFile, FileMode.Open, FileAccess.Read))
				{
					using (var streamreader = new StreamReader(stream))
					{
						using (var csvReader = new CsvReader(streamreader))
						{

							csvReader.Configuration.HasHeaderRecord = false;
							csvReader.Configuration.RegisterClassMap<HousePriceMap>();
							while (await csvReader.ReadAsync())
							{

								var data = csvReader.GetRecord<Services.HousePrice>();
								Log.Debug(JsonConvert.SerializeObject(data));



								await AddToDB(f, data, client);

							}

						}
					}

				}

	            var destFilename = Path.Combine(successDirectory, filename);

                File.Move(processingFile, destFilename);
            }));
			Log.Information("Starting to poll...");
            watcher.StartPolling();

            while (true)
            {

            }

            // ReSharper disable once FunctionNeverReturns
        }

	    private static async Task AddToDB(FileInfo f, Services.HousePrice record, RestClient client)
	    {
		    var req = new RestRequest($"api/transaction");

		    req.AddParameter("application/json", JsonConvert.SerializeObject(record), ParameterType.RequestBody);
		    req.RequestFormat = DataFormat.Json;
			Log.Information("Calling transaction import");
		    var result = await client.ExecutePostTaskAsync(req);
		    if (!result.IsSuccessful)
		    {
			    Log.Error($"post failed, status: {result.StatusCode}, message:{result.ErrorMessage}, content: {result.Content}");
		    }


	    }
    }
}