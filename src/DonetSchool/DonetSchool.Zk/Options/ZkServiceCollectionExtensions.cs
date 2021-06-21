using DonetSchool.Zk;
using DonetSchool.Zk.Listeners;
using DonetSchool.Zk.Options;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ZkServiceCollectionExtensions
    {
        public static IServiceCollection AddZk(this IServiceCollection services, Configuration.IConfiguration configuration, Action<ZkOptions> configure = null)
        {
            var options = new ZkOptions();
            configure?.Invoke(options);
            services
               .AddTransient<IConfigProvider, ConfigProvider>()
               .AddTransient<IZkClientFactory, ZkClientFactory>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>().AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddOptions();
            services.AddHostedService<ListenerHostService>();
            var section = configuration.GetSection(options.SectionName);
            services.Configure<List<ZkConfig>>(section);

            services.AddSingleton(options);
            return services;
        }

        public static IServiceCollection AddZkListener<TListener>(this IServiceCollection services) where TListener : class, IListener
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IListener, TListener>());
            return services;
        }
    }
}