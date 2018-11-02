using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using JetBrains.Annotations;
using Serilog;

namespace HousePrice.Api.ImportFileWatcher
{
    public class FilePoller
    {
        [NotNull] private readonly string _watchPath;
        private readonly Action<FileInfo> _onFileCreate;
        private readonly Action<FileInfo> _onFileModify;
        private readonly Action<FileInfo> _onFileDelete;
        private readonly Action<FileInfo> _onSuccess;
        private readonly Action<string, Exception> _onError;

        public FilePoller([NotNull] string watchPath, Action<FileInfo> onFileCreate = null,
            Action<FileInfo> onFileModify = null,
            Action<FileInfo> onFileDelete = null, Action<FileInfo> onSuccess = null,
            Action<string, Exception> onError = null)
        {
            _watchPath = watchPath;
            _onFileCreate = onFileCreate;
            _onFileModify = onFileModify;
            _onFileDelete = onFileDelete;
            _onSuccess = onSuccess;
            _onError = onError;
           
        }

//        public string FilePath => _watchPath;
//        public Action<FileInfo> OnFileCreate => _onFileCreate;
//        public Action<FileInfo> OnFileModify => _onFileModify;
//        public Action<FileInfo> OnFileDelete => _onFileDelete;

        private DateTime _lastSnapShot = DateTime.MinValue;
        
        private Dictionary<string, FileInfo> _lastState = new Dictionary<string, FileInfo>();

        public void StartPolling(int timeout = 15000)
        {
            var timer = new Timer {Interval = timeout};

	        if (!Directory.Exists(_watchPath))
	        {
		        Log.Warning($"Can't find directory {_watchPath}");
		        return;
	        }

	        _lastState = Directory.GetFiles(_watchPath)
                .Select(f => new FileInfo(f))
                .ToDictionary(k => k.FullName);
            
            timer.Elapsed += (sender, eventArgs) =>
            {
				Log.Information($"Logging timer activated for {_watchPath}");
                var currentFiles = Directory.GetFiles(_watchPath)
                    .Select(f => new FileInfo(f))
                    .ToDictionary(k => k.FullName);

				StringBuilder laststatemessage = new StringBuilder();

	            foreach (var file in _lastState)
	            {

		            laststatemessage.AppendLine(file.Value.FullName);

	            }
	            StringBuilder currentstatemessage = new StringBuilder();
	            foreach (var file in currentFiles)
	            {
		            currentstatemessage.AppendLine(file.Value.FullName);
	            }

				Log.Information($"Last state:\r\n{laststatemessage.ToString()} /r/nCurrent State:/r/n{currentstatemessage.ToString()}");

	            foreach (var watchedFileInfo in currentFiles.Values)
                {
                    Log.Information($"Processing dropped file {watchedFileInfo.Name}");

                    if (!_lastState.ContainsKey(watchedFileInfo.FullName))
                    {
                        _onFileCreate?.Invoke(watchedFileInfo);
                    }
                    else
                    {
                        if (_lastState[watchedFileInfo.FullName].LastAccessTimeUtc <
                            watchedFileInfo.LastWriteTimeUtc)
                        {
                            _onFileModify?.Invoke(watchedFileInfo);
                        }
                    }


                    _onSuccess?.Invoke(watchedFileInfo);
                }

                var deleted = _lastState.Values.Where(v => !currentFiles.ContainsKey(v.FullName)).ToArray();

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

                _lastSnapShot = DateTime.UtcNow;

                _lastState = currentFiles;

            };
   

            timer.Enabled = true;


        // ReSharper disable once FunctionNeverReturns
        }
    }
}