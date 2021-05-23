using DonetSchool.QoS.Config;
using DonetSchool.QoS.Rules;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement
{
    public class QoSProcessor : IQoSProcessor
    {
        private readonly IPollyFactory _pollyFactory;
        private readonly IRuleMatcher _ruleMatcher;
        private readonly IQuotaProcessor _quotaProcessor;

        public QoSProcessor(IPollyFactory pollyFactory, IRuleMatcher ruleMatcher, IQuotaProcessor quotaProcessor)
        {
            _pollyFactory = pollyFactory;
            _ruleMatcher = ruleMatcher;
            _quotaProcessor = quotaProcessor;
        }

        public async Task ExecuteAsync(Func<bool, Context, QoSConfig, CancellationToken, Task> action, RequestInfo requestInfo, Dictionary<string, object> pollyContextData = null)
        {
            await ExecuteAsync(action, requestInfo, cancellationToken: CancellationToken.None, pollyContextData: pollyContextData);
        }

        public async Task ExecuteAsync(Func<bool, Context, QoSConfig, CancellationToken, Task> action, RequestInfo requestInfo, CancellationToken cancellationToken, Dictionary<string, object> pollyContextData = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (_ruleMatcher.IsMatched(requestInfo, out var qoSConfig))
            {
                var requestKey = requestInfo.ToString();
                var policy = _pollyFactory.Get(requestKey, qoSConfig.BreakConfig);
                await policy.ExecuteAsync(async (pollyContext, cancellationToken) =>
                {
                    if (qoSConfig.QuotaConfig != null)
                    {
                        await _quotaProcessor.ProcessAsync(requestKey, qoSConfig.QuotaConfig);
                    }
                    await action.Invoke(true, pollyContext, qoSConfig, cancellationToken);
                }, _pollyFactory.CreatePollyContext(requestKey, pollyContextData), cancellationToken);
            }
            else
            {
                await action.Invoke(false, null, null, cancellationToken);
            }
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<bool, Context, QoSConfig, CancellationToken, Task<TResult>> action, RequestInfo requestInfo, Dictionary<string, object> pollyContextData = null)
        {
            return await ExecuteAsync(action, requestInfo, cancellationToken: CancellationToken.None, pollyContextData: pollyContextData);
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<bool, Context, QoSConfig, CancellationToken, Task<TResult>> action, RequestInfo requestInfo, CancellationToken cancellationToken, Dictionary<string, object> pollyContextData = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (_ruleMatcher.IsMatched(requestInfo, out var qoSConfig))
            {
                var requestKey = requestInfo.ToString();
                var policy = _pollyFactory.Get(requestKey, qoSConfig.BreakConfig);
                return await policy.ExecuteAsync(async (pollyContext, cancellationToken) =>
                {
                    if (qoSConfig.QuotaConfig != null)
                    {
                        await _quotaProcessor.ProcessAsync(requestKey, qoSConfig.QuotaConfig);
                    }
                    return await action.Invoke(true, pollyContext, qoSConfig, cancellationToken);
                }, _pollyFactory.CreatePollyContext(requestKey, pollyContextData), cancellationToken);
            }
            else
            {
                return await action.Invoke(false, null, null, cancellationToken);
            }
        }
    }
}