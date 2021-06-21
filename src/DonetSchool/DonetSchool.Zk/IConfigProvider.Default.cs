using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace DonetSchool.Zk
{
    public class ConfigProvider : IConfigProvider
    {
        private readonly IOptionsMonitor<List<ZkConfig>> _optionsMonitor;

        public ConfigProvider(IOptionsMonitor<List<ZkConfig>> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public ZkConfig GetConfig(string name)
        {
            var currentValue = _optionsMonitor.CurrentValue;
            return currentValue?.FirstOrDefault(m => m.Name == name);
        }
    }
}