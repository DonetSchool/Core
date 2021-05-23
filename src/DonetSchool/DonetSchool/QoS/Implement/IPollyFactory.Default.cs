using DonetSchool.QoS.Config;
using DonetSchool.QoS.Delegates;
using DonetSchool.QoS.Exceptions;
using DonetSchool.QoS.Extensions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement
{
    public class PollyFactory : IPollyFactory
    {
        private static readonly ConcurrentDictionary<string, IAsyncPolicy> _PolicyCache = new ConcurrentDictionary<string, IAsyncPolicy>();

        private readonly ILogger _logger;

        public PollyFactory(ILogger<PollyFactory> logger)
        {
            _logger = logger;
        }

        public Context CreatePollyContext(string requestIdentity, Dictionary<string, object> contextData = null)
        {
            var context = new Context()
            {
                { "RequestIdentity", requestIdentity },
                { "Logger", _logger }
            };
            if (contextData != null)
            {
                foreach (var item in contextData)
                {
                    context.Add(item.Key, item.Value);
                }
            }
            return context;
        }

        public IAsyncPolicy Get(string requestIdentity, BreakConfig qosConfig)
        {
            if (string.IsNullOrWhiteSpace(requestIdentity) || qosConfig == null)
            {
                return Policy.NoOpAsync();
            }
            if (!_PolicyCache.TryGetValue(requestIdentity, out var policy))
            {
                lock (_PolicyCache)
                {
                    if (!_PolicyCache.TryGetValue(requestIdentity, out policy))
                    {
                        policy = CreatePolicy(qosConfig);
                        _PolicyCache[requestIdentity] = policy;
                    }
                }
            }
            return policy;
        }

        private IAsyncPolicy CreatePolicy(BreakConfig breakConfig)
        {
            IAsyncPolicy policy = Policy.NoOpAsync();
            if (breakConfig != null)
            {
                policy = policy.WrapAsync(Policy.Handle<TimeoutException>()
                    .Or<TimeoutRejectedException>()
                    .Or<BrokenCircuitException>()
                    .Or<ThrottlException>()
                    .FallbackAsync(fallbackAction: async (exception, context, cancellationToken) =>
                    {
                        if (QoSEvents.OnFallbackAction != null)
                        {
                            await QoSEvents.OnFallbackAction.Invoke(exception, context, cancellationToken);
                        }
                        else
                        {
                            throw new ThrottlException();
                        }
                    }, onFallbackAsync: async (exception, context) =>
                    {
                        if (QoSEvents.OnFallback != null)
                        {
                            await QoSEvents.OnFallback.Invoke(exception, context);
                        }
                    }));

                var breakPolicy = Policy.Handle<TimeoutException>()
                    .Or<TimeoutRejectedException>()
                    .Or<TaskCanceledException>()
                    .AdvancedCircuitBreakerAsync(
                            failureThreshold: breakConfig.FailureThreshold,
                            samplingDuration: TimeSpan.FromSeconds(breakConfig.SamplingDuration),
                            minimumThroughput: breakConfig.MinimumThroughput,
                            durationOfBreak: TimeSpan.FromSeconds(breakConfig.DurationOfBreak),
                            onBreak: (exception, circuitState, timeSpan, context) =>
                            {
                                context.GetLogger()?.LogError(exception, "Start break request {0} and end break operation after {1} seconds.", context.GetRequestIdentity(), timeSpan.TotalSeconds);
                                QoSEvents.OnBreakEvent?.Invoke(exception, circuitState, timeSpan, context);
                            },
                            onReset: (context) =>
                            {
                                context.GetLogger()?.LogInformation("Reset break request {0}.", context.GetRequestIdentity());
                                QoSEvents.OnResetEvent?.Invoke(context);
                            },
                            onHalfOpen: () =>
                            {
                                QoSEvents.OnHalfOpen?.Invoke();
                            }
                            );
                policy = policy.WrapAsync(breakPolicy);
                if (breakConfig.Timeout > 0)
                {
                    policy = policy.WrapAsync(Policy.TimeoutAsync(breakConfig.Timeout, TimeoutStrategy.Pessimistic));
                }
            }

            return policy;
        }

        public static void Reset()
        {
            lock (_PolicyCache)
            {
                _PolicyCache.Clear();
            }
        }
    }
}