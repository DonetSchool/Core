using DonetSchool.QoS.Config;
using DonetSchool.QoS.Delegates;
using DonetSchool.QoS.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement
{
    public class QuotaProcessor : IQuotaProcessor
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IQuotaHandler> _quotaHandlers;

        public QuotaProcessor(ILogger<QuotaProcessor> logger, IEnumerable<IQuotaHandler> quotaHandlers)
        {
            _logger = logger;
            _quotaHandlers = quotaHandlers;
        }

        public async Task ProcessAsync(string requestIdentity, QuotaConfig quotaConfig)
        {
            if (string.IsNullOrWhiteSpace(requestIdentity) || quotaConfig == null)
            {
                return;
            }
            var handler = _quotaHandlers.FirstOrDefault(m => m.RuleType == quotaConfig.RuleType);
            if (handler == null)
            {
                throw new NotSupportedException($"cannot found the rate litimt handler of type {quotaConfig.RuleType}.");
            }
            var (isAllow, waittimeMills) = await handler.IsAllowRequestAsync(requestIdentity, quotaConfig);
            QoSEvents.OnLimitProcessResult?.Invoke(requestIdentity, quotaConfig, isAllow, waittimeMills);
            if (isAllow)
            {
                if (waittimeMills > 0)
                {
                    await Task.Delay(waittimeMills);
                }
            }
            else
            {
                throw new ThrottlException();
            }
        }
    }
}