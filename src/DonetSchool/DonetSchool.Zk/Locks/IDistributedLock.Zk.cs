namespace DonetSchool.Zk.Locks
{
    internal class ZkDistributedLock : IDistributedLock
    {
        private readonly ZkClient _zkClient;
        private readonly ZkLock _zkLock;

        public ZkDistributedLock(ZkClient zkClient, ZkLock zkLock)
        {
            _zkLock = zkLock;
            _zkClient = zkClient;
        }

        public string Key => _zkLock.Dir;

        public void Dispose()
        {
            _zkClient.UnLockAsync(_zkLock).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}