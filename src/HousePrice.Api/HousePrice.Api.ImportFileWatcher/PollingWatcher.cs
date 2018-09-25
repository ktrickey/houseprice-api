using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace HousePrice.Api.ImportFileWatcher
{
	public class PollingWatcher
	{
		[NotNull] private readonly string _watchPath;
		private readonly Action<FileInfo> _onFilecreate;
		private readonly Action<FileInfo> _onFileModify;
		private readonly Action<FileInfo> _onFileDelete;
		private readonly Action<FileInfo> _onSuccess;
		private readonly Action<string, Exception> _onError;
		private readonly FileSystemWatcher _watcher;

		public PollingWatcher([NotNull] string watchPath, Action<FileInfo> onFilecreate = null, Action<FileInfo> onFileModify = null,
			Action<FileInfo> onFileDelete =null, Action<FileInfo> onSuccess = null, Action<string, Exception> onError = null)
		{
			_watchPath = watchPath;
			_onFilecreate = onFilecreate;
			_onFileModify = onFileModify;
			_onFileDelete = onFileDelete;
			_onSuccess = onSuccess;
			_onError = onError;
			_watcher = new FileSystemWatcher(watchPath);

		}

		private DateTime _lastSnapShot = DateTime.MinValue;
		private Dictionary<string, FileInfo> lastState = new Dictionary<string, FileInfo>();
		public async Task StartPolling()
		{

			while (true)
			{
				await Task.Delay(10000);
				var currentFiles = Directory.GetFiles(_watchPath).Select(f => new FileInfo(f))
					.ToDictionary(k => k.FullName);


				foreach (var watchedFileInfo in currentFiles.Values)
				{


					Console.WriteLine($"Processing {watchedFileInfo.Name}");

					if (!lastState.ContainsKey(watchedFileInfo.FullName))
					{
						_onFilecreate?.Invoke(watchedFileInfo);
					}
					else
					{
						if (lastState[watchedFileInfo.FullName].LastAccessTimeUtc < watchedFileInfo.LastWriteTimeUtc)
						{
							_onFileModify?.Invoke(watchedFileInfo);
						}
					}


					_onSuccess?.Invoke(watchedFileInfo);
				}

				var deleted = lastState.Values.Where(v => !currentFiles.ContainsKey(v.FullName)).ToArray();

				foreach (var deletedItem in deleted)
				{
					try
					{
						_onFileDelete?.Invoke(deletedItem);
						_onSuccess?.Invoke(deletedItem);
					}
					catch (Exception e)
					{
						_onError?.Invoke(deletedItem.FullName, e);
					}


				}


			}
			// ReSharper disable once FunctionNeverReturns
		}
	}
}