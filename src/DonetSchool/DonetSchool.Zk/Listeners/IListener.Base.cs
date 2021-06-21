using DonetSchool.Zk.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DonetSchool.Zk.Listeners
{
    public abstract class BaseListener : IListener
    {
        protected readonly ILogger _logger;

        public BaseListener(IServiceProvider serviceProvider) : this(serviceProvider.GetService<ZkOptions>().DefaultName, serviceProvider)
        {
        }

        public BaseListener(string configName, IServiceProvider serviceProvider) : this(serviceProvider.GetService<IConfigProvider>().GetConfig(configName), serviceProvider)
        {
        }

        public BaseListener(ZkConfig zkConfig, IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<BaseListener>>();
            Config = zkConfig ?? throw new ArgumentNullException(nameof(zkConfig));
            Client = serviceProvider.GetService<IZkClientFactory>().CreateNew(zkConfig, WatchNotice);
        }

        public ZkConfig Config { get; }

        protected ZkClient Client { get; }

        public abstract string[] WatchPaths { get; }

        public virtual bool IsWatchChildren => true;

        public virtual async Task StartAsync()
        {
            await StartWatchAsync();
        }

        public async Task StartWatchAsync()
        {
            if (WatchPaths != null && WatchPaths.Any())
            {
                foreach (var path in WatchPaths)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await Client.ExistsAsync(path, watch: true);
                        if (IsWatchChildren)
                        {
                            await Client.GetChildrenListAsync(path, watch: true);
                        }
                    }
                }
            }
        }

        public virtual Task StopAsync()
        {
            _logger.LogInformation("执行释放");
            Client?.Dispose();
            return Task.CompletedTask;
        }

        public virtual async Task WatchNotice(ZkClient client, WatchedEvent @event)
        {
            try
            {
                await OnWatchNotice(Client, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "handle watch error.");
            }
            try
            {
                await StartWatchAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "start watch error.");
            }
        }

        public abstract Task OnWatchNotice(ZkClient client, WatchedEvent @event);
    }
}