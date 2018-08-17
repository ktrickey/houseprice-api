using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HousePrice.Api.Services;

namespace HousePrice.Api.ImportFileWatcher
{
	class Program
	{
		static void Main(string[] args)
		{

			//Monitor the drop directory for new files - Docker will map to file system as will k8s
			var watchDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"Client\\{args[0]}\\Import\\Drop");
			var successDirectory = Path.Combine(Directory.GetCurrentDirectory(), $"Client\\{args[0]}\\Import\\Complete");
			FileSystemWatcher watcher = new FileSystemWatcher(watchDirectory );
			PostcodeLookup.GetByPostcode("");
			var importer = new Importer();
			while (true)
			{
				var watch = watcher.WaitForChanged(WatcherChangeTypes.Created);
				var watchedFile = Path.Combine(watchDirectory, watch.Name);
				using (var fileStream = new FileStream(watchedFile, FileMode.Open, FileAccess.Read))
				{
					importer.Import(fileStream);
				}

				File.Move(watchedFile, Path.Combine(successDirectory, watch.Name));

			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}
