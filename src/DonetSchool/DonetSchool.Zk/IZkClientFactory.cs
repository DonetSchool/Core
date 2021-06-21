using org.apache.zookeeper;
using System;
using System.Threading.Tasks;

namespace DonetSchool.Zk
{
    public interface IZkClientFactory
    {
        ZkClient this[string name] { get; }

        ZkClient CreateDefaultNew(Func<ZkClient, WatchedEvent, Task> noticeAction = null);

        ZkClient CreateNew(string configName, Func<ZkClient, WatchedEvent, Task> noticeAction = null);

        ZkClient CreateNew(ZkConfig config, Func<ZkClient, WatchedEvent, Task> noticeAction = null);

        ZkClient Get(string name);

        ZkClient GetDefault();
    }
}