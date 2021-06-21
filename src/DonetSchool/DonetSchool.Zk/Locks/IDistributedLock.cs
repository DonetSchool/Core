using System;

namespace DonetSchool.Zk.Locks
{
    public interface IDistributedLock : IDisposable
    {
        string Key { get; }
    }
}