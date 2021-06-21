using org.apache.zookeeper;
using System.Threading.Tasks;

namespace DonetSchool.Zk
{
    public interface IWatcher
    {
        Task ProcessAsync(ZkClient client, WatchedEvent @event);
    }
}