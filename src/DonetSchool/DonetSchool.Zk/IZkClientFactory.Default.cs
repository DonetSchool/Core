using DonetSchool.Zk.Options;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DonetSchool.Zk
{
    public class ZkClientFactory : IZkClientFactory
    {
        private static readonly ConcurrentDictionary<string, ZkClient> _Clients = new ConcurrentDictionary<string, ZkClient>();

        private readonly IConfigProvider _configProvider;
        private readonly ZkOptions _zkOptions;
        private readonly ILogger _logger;

        public ZkClientFactory(IConfigProvider configProvider, ZkOptions zkOptions, ILogger<ZkClientFactory> logger)
        {
            _configProvider = configProvider;
            _zkOptions = zkOptions;
            _logger = logger;
        }

        public ZkClient this[string name]
        {
            get
            {
                return Get(name);
            }
        }

        public ZkClient GetDefault()
        {
            var defaultName = _zkOptions.DefaultName;
            return Get(defaultName);
        }

        public ZkClient Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (!_Clients.TryGetValue(name, out var client))
            {
                lock (_Clients)
                {
                    if (!_Clients.TryGetValue(name, out client))
                    {
                        client = CreatImplement(name);
                        _Clients[name] = client;
                    }
                }
            }
            return client;
        }

        private ZkClient CreatImplement(string name)
        {
            var config = _configProvider.GetConfig(name);
            if (config == null)
            {
                throw new ArgumentNullException($"Cannot found the config of {name}.");
            }
            var client = new ZkClient(config, null, _logger);
            return client;
        }

        public ZkClient CreateDefaultNew(Func<ZkClient, WatchedEvent, Task> @noticeAction = null)
        {
            var defaultName = _zkOptions.DefaultName;
            return CreateNew(defaultName, @noticeAction: @noticeAction);
        }

        public ZkClient CreateNew(string configName, Func<ZkClient, WatchedEvent, Task> @noticeAction = null)
        {
            var config = _configProvider.GetConfig(configName);
            return CreateNew(config, @noticeAction);
        }

        public ZkClient CreateNew(ZkConfig config, Func<ZkClient, WatchedEvent, Task> @noticeAction = null)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            return new ZkClient(config, @noticeAction, _logger);
        }
    }
}