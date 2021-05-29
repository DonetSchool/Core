using DonetSchool.QoS;
using DonetSchool.QoS.Delegates;
using DonetSchool.QoS.Implement;
using DonetSchool.QoS.Implement.Handlers;
using DonetSchool.QoS.Options;
using DonetSchool.QoS.Rules;
using DonetSchool.QoS.Rules.Implement;
using DonetSchool.QoS.Services;
using DonetSchool.QoS.Services.Implement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class QoSServiceCollectionExtensions
    {
        public static IServiceCollection AddQoS(this IServiceCollection services, IConfiguration configuration, Action<QoSOptionsBuilder> configure = null)
        {
            #region Events

            var builder = new QoSOptionsBuilder();
            configure?.Invoke(builder);
            builder.InstallEvents();

            #endregion Events

            services.AddMemoryCache();
            services.AddOptions();
            services.Configure<QoSConfigureOptions>(configuration);

            services.AddTransient<IPollyFactory, PollyFactory>();
            services.AddTransient<IQoSProcessor, QoSProcessor>();
            services.AddTransient<IQuotaProcessor, QuotaProcessor>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IQuotaHandler, FixedWindowHandler>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IQuotaHandler, LeakyBucketQuotaHandler>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IQuotaHandler, SlidingWindowQuotaHandler>());
            services.TryAddEnumerable(ServiceDescriptor.Transient<IQuotaHandler, TokenBucketQuotaHandler>());

            services.AddTransient<IRuleMatcher, RuleMatcher>();
            services.AddSingleton<IRuleProvider, MemoryRuleProvider>();
            services.AddTransient<ICounterStoreService, MemoryCounterStoreService>();
            services.AddTransient<ILockerService, MonitorLockerService>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>().AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            return services;
        }
    }
}