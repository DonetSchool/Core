using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.Zk.Listeners
{
    public class ListenerHostService : BackgroundService
    {
        private readonly IEnumerable<IListener> _listeners;

        public ListenerHostService(IServiceProvider serviceProvider)
        {
            _listeners = serviceProvider.GetService<IEnumerable<IListener>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_listeners != null && _listeners.Any())
            {
                foreach (var listener in _listeners)
                {
                    await listener.StartAsync();
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_listeners != null && _listeners.Any())
            {
                foreach (var listener in _listeners)
                {
                    await listener.StopAsync();
                }
            }

            await base.StopAsync(cancellationToken);
        }
    }
}