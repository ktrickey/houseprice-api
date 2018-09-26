using System.IO;
using System.Threading.Tasks;

namespace HousePrice.Api.ImportFileWatcher
{
    public class PollingWatcher
    {
        private readonly FilePoller _poller;
        private readonly FileSystemWatcher _watcher;
        public PollingWatcher(FilePoller poller)
        {
            _poller = poller;
            
            _watcher = new FileSystemWatcher(poller.FilePath);
            if (poller.OnFileModify != null)
            {
                _watcher.Changed += (sender, eventArgs) => { poller.OnFileModify(new FileInfo(eventArgs.FullPath)); };
            }

            if (poller.OnFileCreate != null)
            {
                _watcher.Created += (sender, eventArgs) => { poller.OnFileCreate(new FileInfo(eventArgs.FullPath)); };
            }

            if (poller.OnFileDelete != null)
            {
                _watcher.Deleted += (sender, eventArgs) => { poller.OnFileDelete(new FileInfo(eventArgs.FullPath)); };
            }
        }

        public async Task StartPolling()
        {
            _watcher.EnableRaisingEvents = true;

            _poller.StartPolling();
        }
    }
}