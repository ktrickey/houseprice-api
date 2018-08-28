using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using Microsoft.Extensions.Configuration;

namespace HousePrice.Api.ImportFileWatcher
{
	class Program
	{
		static void Main()
		{

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables();

			var configuration = builder.Build();


			
			
			//Monitor the drop directory for new files - Docker will map to file system as will k8s
			var watchDirectory = "/transaction_data/Import/Drop";
			var successDirectory = "/transaction_data/Import/Complete";
			var files = Directory.GetFiles(watchDirectory);
			FileSystemWatcher watcher = new FileSystemWatcher(watchDirectory );
			PostcodeLookup.GetByPostcode("");
			var importer = new Importer();
			while (true)
			{
				Thread.Sleep(10000);
				var currentFiles = Directory.GetFiles(watchDirectory);


				//var watch = watcher.WaitForChanged(WatcherChangeTypes.All);
				//var watchedFile = Path.Combine(watchDirectory, watch.Name);

				foreach (var watchedFile in currentFiles)
				{

					var info = new FileInfo(watchedFile);
					Console.WriteLine($"Processing {info.Name}");
					using (var fileStream = new FileStream(watchedFile, FileMode.Open, FileAccess.Read))
					{
						importer.Import(fileStream);
					}
					File.Move(watchedFile, Path.Combine(successDirectory, info.Name));
				}

				

			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}
