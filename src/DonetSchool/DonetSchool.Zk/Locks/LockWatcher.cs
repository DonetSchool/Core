using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DonetSchool.Zk.Locks
{
    internal class LockWatcher : IWatcher
    {
        private readonly ZkLock _zkLock;

        public LockWatcher(ZkLock zkLock)
        {
            _zkLock = zkLock;
        }

        public async Task ProcessAsync(ZkClient client, WatchedEvent @event)
        {
            if (!_zkLock.IsCancel)
            {
                switch (@event.getState())
                {
                    case Watcher.Event.KeeperState.Disconnected:
                    case Watcher.Event.KeeperState.Expired:
                        await client.SetGetLockFailedResultAsync(_zkLock);
                        return;
                }
                await client.GetLockAsync(_zkLock);
            }
        }
    }
}
