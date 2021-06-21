using System.Threading.Tasks;

namespace DonetSchool.Zk.Listeners
{
    public interface IListener
    {
        string[] WatchPaths { get; }

        bool IsWatchChildren { get; }

        Task StartAsync();

        Task StopAsync();
    }
}