using DonetSchool.Zk.Locks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.Zk
{
    public class ZkClient : IDisposable
    {
        private readonly Func<ZkClient, WatchedEvent, Task> _changeEvent;
        private readonly ILogger _logger;
        private ZooKeeper _zooKeeper;

        public ZkClient(ZkConfig config, Func<ZkClient, WatchedEvent, Task> @event, ILogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _changeEvent = @event;
            _logger = logger;
        }

        public ZkConfig Config { get; }

        public ZooKeeper ZkInstance
        {
            get
            {
                if (_zooKeeper == null || _zooKeeper.getState() != ZooKeeper.States.CONNECTED)
                {
                    _zooKeeper = CreateZk();
                }
                return _zooKeeper;
            }
        }

        private ZooKeeper CreateZk()
        {
            ZooKeeper.LogToFile = false;
            var zk = new ZooKeeper(Config.ConnectionString, Config.SessionTimeout, new ZkWatcher(this), Config.SessionId, Config.SessionPasswdBytes, Config.ReadOnly);
            int currentTryCount = 0;
            while (zk.getState() != ZooKeeper.States.CONNECTED && currentTryCount < Config.RetryCount)
            {
                System.Threading.Thread.Sleep(1000);
            }
            return zk;
        }

        public async Task<bool> ExistsAsync(string sourcePath, bool watch = false)
        {
            var stat = await ZkInstance.existsAsync(sourcePath, watch: watch);
            return stat != null;
        }

        public async Task<bool> ExistsAsync(string sourcePath, IWatcher watcher)
        {
            var stat = await ZkInstance.existsAsync(sourcePath, watcher: new DefaultWatcher(this, watcher));
            return stat != null;
        }

        public async Task<byte[]> GetDataAsync(string sourcePath, bool watch = false)
        {
            if (await ExistsAsync(sourcePath, watch: false))
            {
                var dataResult = await ZkInstance.getDataAsync(sourcePath, watch: watch);
                return dataResult.Data;
            }
            return null;
        }

        public async Task<byte[]> GetDataAsync(string sourcePath, IWatcher watcher)
        {
            if (await ExistsAsync(sourcePath, watcher))
            {
                var dataResult = await ZkInstance.getDataAsync(sourcePath, watcher: new DefaultWatcher(this, watcher));
                return dataResult.Data;
            }
            return null;
        }

        public async Task<string> CreateAsync(string path, byte[] data, CreateMode createMode = null)
        {
            return await CreateAsync(path, data, ZooDefs.Ids.OPEN_ACL_UNSAFE, createMode ?? CreateMode.PERSISTENT);
        }

        public async Task<string> CreateAsync(string path, byte[] data, List<ACL> acl, CreateMode createMode)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }
            string currentPath = string.Empty;
            string lastCreatedPath = string.Empty;
            var pathList = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathList.Length; i++)
            {
                currentPath += "/" + pathList[i];
                var isExists = await ExistsAsync(currentPath);
                if (!isExists)
                {
                    if (i == pathList.Length - 1)
                    {
                        lastCreatedPath = await ZkInstance.createAsync(currentPath, data, acl, createMode);
                    }
                    else
                    {
                        await ZkInstance.createAsync(currentPath, data, acl, CreateMode.PERSISTENT);
                    }
                }
                else if (i == pathList.Length - 1)
                {
                    await ZkInstance.setDataAsync(currentPath, data);
                    lastCreatedPath = currentPath;
                }
            }

            return lastCreatedPath;
        }

        public async Task<string> SetDataAsync(string path, byte[] data, CreateMode createMode = null)
        {
            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }
            if (await ExistsAsync(path))
            {
                await ZkInstance.setDataAsync(path, data);
                return path;
            }
            else
            {
                return await CreateAsync(path, data, createMode ?? CreateMode.EPHEMERAL);
            }
        }

        public async Task<List<string>> GetChildrenListAsync(string path, bool watch = false)
        {
            var isExists = await ExistsAsync(path, watch: watch);
            if (!isExists)
            {
                return new List<string>();
            }
            var childrenResult = await ZkInstance.getChildrenAsync(path, watch: watch);
            return childrenResult.Children;
        }

        public async Task<List<string>> GetChildrenListAsync(string path, IWatcher watcher)
        {
            var isExists = await ExistsAsync(path, watcher);
            if (!isExists)
            {
                return new List<string>();
            }
            var childrenResult = await ZkInstance.getChildrenAsync(path, watcher: new DefaultWatcher(this, watcher));
            return childrenResult.Children;
        }

        public async Task<bool> DeleteAsync(string sourcePath)
        {
            if (await ExistsAsync(sourcePath))
            {
                await ZkInstance.deleteAsync(sourcePath);
                return true;
            }
            return false;
        }

        // /Lock/zkTest /Lock/zkTest/A-000001 /Lock/zkTest/A-000002 /Lock/zkTest/A-000003

        public async Task<(bool IsSuccess, IDistributedLock DistributedLock)> GetLockAsync(string keyPath, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(keyPath))
            {
                throw new ArgumentNullException(nameof(keyPath));
            }

            var perfix = ZkInstance.getSessionId() + "-";
            var lockPath = GetFullPath(keyPath, perfix);
            var lockPathNo = await CreateAsync(lockPath, null, createMode: CreateMode.EPHEMERAL_SEQUENTIAL);
            // 首先获取当前文件夹下面所有节点 1
            // 获取最小节点信息  然后把最小节点信息与自己拿到的锁路径进行对比 2
            // 如果是一样的，那么就表示我获得了锁 3
            // 那如果不是一样的 我就继续等待 并监听改节点下的子节点信息变更通知4
            // 接收到通知之后我再获取所有节点 1
            var cancellationTokenSource = new CancellationTokenSource(timeout);
            var zkLock = new ZkLock(keyPath, lockPathNo, new TaskCompletionSource<(bool, IDistributedLock)>(), cancellationTokenSource);
            cancellationTokenSource.Token.Register(() =>
            {
                //TODO 设置获取锁失败 并且删除当前锁的节点
                try
                {
                    SetGetLockFailedResultAsync(zkLock).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "zk lock cancel error.");
                }
            });
            // 执行上面的逻辑
            await GetLockAsync(zkLock);
          
            return await zkLock.TaskCompletionSource.Task;
        }

        internal async Task GetLockAsync(ZkLock zkLock)
        {
            if (!zkLock.IsCancel)
            {
                var childrenList = await GetChildrenListAsync(zkLock.Dir);
                if (childrenList != null && childrenList.Any())
                {
                    SortedSet<ZLockNodeName> sortedNames = new SortedSet<ZLockNodeName>();
                    foreach (string name in childrenList)
                    {
                        sortedNames.Add(new ZLockNodeName(GetFullPath(zkLock.Dir, name)));
                    }
                    zkLock.CurrentOwner = sortedNames.Min.Name;

                    if (zkLock.IsOwner)
                    {
                        //设置获取锁成功
                        zkLock.SetResult((true, new ZkDistributedLock(this, zkLock)));
                        return;
                    }
                }
                await GetChildrenListAsync(zkLock.Dir, new LockWatcher(zkLock));
            }
        }

        internal async Task SetGetLockFailedResultAsync(ZkLock zkLock)
        {
            zkLock.SetResult((false, null));
            await UnLockAsync(zkLock);
        }

        internal async Task UnLockAsync(ZkLock zkLock)
        {
            if (zkLock != null && zkLock.NodeName != null && !string.IsNullOrWhiteSpace(zkLock.NodeName.Name))
            {
                await DeleteAsync(zkLock.NodeName.Name);
            }
        }

        public string GetFullPath(string parentPath, string currentName)
        {
            if (parentPath.EndsWith("/"))
            {
                return parentPath + currentName;
            }
            return parentPath + "/" + currentName;
        }

        public Task Sync(string path)
        {
            return ZkInstance.sync(path);
        }

        public void Dispose()
        {
            if (_zooKeeper != null && _zooKeeper.getState() != ZooKeeper.States.CLOSED)
            {
                _zooKeeper.closeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        private class ZkWatcher : Watcher
        {
            private readonly ZkClient _zkClient;

            public ZkWatcher(ZkClient zkClient)
            {
                _zkClient = zkClient;
            }

            public override async Task process(WatchedEvent @event)
            {
                try
                {
                    if (_zkClient._changeEvent != null)
                    {
                        await _zkClient._changeEvent.Invoke(_zkClient, @event);
                    }
                }
                catch (Exception ex)
                {
                    _zkClient._logger.LogError(ex, "deal zk watcher error.");
                }
            }
        }

        private class DefaultWatcher : Watcher
        {
            private readonly ZkClient _zkClient;
            private readonly IWatcher _watcher;

            public DefaultWatcher(ZkClient zkClient, IWatcher watcher)
            {
                _zkClient = zkClient;
                _watcher = watcher;
            }

            public override async Task process(WatchedEvent @event)
            {
                try
                {
                    await _watcher?.ProcessAsync(_zkClient, @event);
                }
                catch (Exception ex)
                {
                    _zkClient._logger.LogError(ex, "deal default watcher error.");
                }
            }
        }
    }
}