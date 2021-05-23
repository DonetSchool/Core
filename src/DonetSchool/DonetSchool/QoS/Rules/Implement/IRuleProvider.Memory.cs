using DonetSchool.Extensions;
using DonetSchool.QoS.Config;
using DonetSchool.QoS.Delegates;
using DonetSchool.QoS.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DonetSchool.QoS.Rules.Implement
{
    public class MemoryRuleProvider : IRuleProvider
    {
        private static List<QoSConfig> _QoSConfigList = null;
        private static readonly object o_lock = new object();
        private static int IsListened = 0;

        private readonly ILogger _logger;
        private readonly IOptionsMonitor<QoSConfigureOptions> _optionsMonitor;

        public MemoryRuleProvider(ILogger<MemoryRuleProvider> logger, IOptionsMonitor<QoSConfigureOptions> optionsMonitor)
        {
            _logger = logger;
            _optionsMonitor = optionsMonitor;
        }

        public List<QoSConfig> GetConfigs()
        {
            if (_QoSConfigList == null)
            {
                ListenChange();
                InstallRules(_optionsMonitor.CurrentValue?.QoS);
            }
            return _QoSConfigList;
        }

        private void ListenChange()
        {
            if (Interlocked.CompareExchange(ref IsListened, 1, 0) == 0)
            {
                _optionsMonitor.OnChange(options =>
                {
                    this.InstallRules(options.QoS);
                });
            }
        }

        public void InstallRules(Dictionary<string, QoSOptions> qoSOptions)
        {
            try
            {
                lock (o_lock)
                {
                    if (qoSOptions == null)
                    {
                        _QoSConfigList = new List<QoSConfig>();
                    }
                    else
                    {
                        List<QoSConfig> list = new List<QoSConfig>();
                        foreach (KeyValuePair<string, QoSOptions> item in qoSOptions)
                        {
                            QoSConfig config = new QoSConfig
                            {
                                MatchConfig = ResolveKey(item.Key)
                            };
                            if (item.Value == null)
                            {
                                throw new ArgumentNullException(string.Format("Not support config of key:{0}", item.Key));
                            }
                            if (item.Value.QoS != null)
                            {
                                Check(item.Value.QoS);
                                config.BreakConfig = new BreakConfig
                                {
                                    DurationOfBreak = item.Value.QoS.DurationOfBreak,
                                    FailureThreshold = item.Value.QoS.FailureThreshold,
                                    MinimumThroughput = item.Value.QoS.MinimumThroughput,
                                    SamplingDuration = item.Value.QoS.SamplingDuration,
                                    Timeout = item.Value.QoS.Timeout
                                };
                            }
                            if (item.Value.Quota != null)
                            {
                                config.QuotaConfig = ResolveQuota(item.Value.Quota);
                            }
                            list.Add(config);
                        }
                        _QoSConfigList = list.OrderBy(m => m.MatchConfig.MatchType).ToList();
                    }

                    _logger.LogInformation("Reset qos config:", _QoSConfigList.ToJosn());
                    if (QoSEvents.RuleResetEvent != null)
                    {
                        try
                        {
                            QoSEvents.RuleResetEvent.Invoke(_QoSConfigList);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Excute rule reset events error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "install rules error of config:{0}", qoSOptions.ToJosn());
            }
        }

        private RuleMatchConfig ResolveKey(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new NotSupportedException(string.Format("Not support serialize key:{0}", template));
            }
            var array = template.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (array.Length > 3)
            {
                throw new NotSupportedException(string.Format("Not support serialize key:{0}", template));
            }
            if (array.Length == 1)
            {
                return new RuleMatchConfig()
                {
                    IsAllMethod = true,
                    MatchType = MatchType.StartWith,
                    Pattern = array[0]
                };
            }

            if (array.Length == 2)
            {
                if (array[1].StartsWith("m:"))
                {
                    return new RuleMatchConfig()
                    {
                        IsAllMethod = false,
                        MatchType = MatchType.StartWith,
                        Methods = GetMethods(array[1]),
                        Pattern = array[0]
                    };
                }
                else
                {
                    return new RuleMatchConfig()
                    {
                        IsAllMethod = true,
                        MatchType = GetMatchType(array[0]),
                        Pattern = array[1]
                    };
                }
            }

            return new RuleMatchConfig()
            {
                IsAllMethod = false,
                MatchType = GetMatchType(array[0]),
                Methods = GetMethods(array[2]),
                Pattern = array[1]
            };
        }

        private string[] GetMethods(string str)
        {
            if (!str.StartsWith("m:"))
            {
                throw new NotSupportedException(string.Format("Not support serialize key match method:{0}", str));
            }
            return str.Replace("m:", "").Split(",", StringSplitOptions.RemoveEmptyEntries);
        }

        private MatchType GetMatchType(string str)
        {
            return str switch
            {
                "=" => MatchType.Equal,
                "^~" => MatchType.StartWith,
                "!^~" => MatchType.StartWith,
                "~" => MatchType.Regex,
                "~*" => MatchType.RegexIgnoreCase,
                "!~" => MatchType.NotRegex,
                "!~*" => MatchType.NotRegexIgnoreCase,
                _ => throw new NotSupportedException(string.Format("Not support serialize key match type:{0}", str)),
            };
        }

        private void Check(BreakOptions qoS)
        {
            if (qoS.Timeout < 0)
            {
                throw new NotSupportedException("qos timeout must be greater than or equal to 0.");
            }
            if (qoS.FailureThreshold <= 0 || qoS.FailureThreshold >= 1)
            {
                throw new NotSupportedException("qos FailureThreshold must be between 0 and, excluding 0 and 1.");
            }
            if (qoS.SamplingDuration <= 0)
            {
                throw new NotSupportedException("qos SamplingDuration must be greater than or equal to 0.");
            }
            if (qoS.MinimumThroughput <= 0)
            {
                throw new NotSupportedException("qos MinimumThroughput must be greater than or equal to 0.");
            }
            if (qoS.DurationOfBreak <= 0)
            {
                throw new NotSupportedException("qos DurationOfBreak must be greater than or equal to 0.");
            }
        }

        private QuotaConfig ResolveQuota(QuotaOptions quotaOptions)
        {
            if (string.IsNullOrWhiteSpace(quotaOptions.Period))
            {
                throw new NotSupportedException("Quota Period must be not empty.");
            }
            if (quotaOptions.Count < 0)
            {
                throw new NotSupportedException("Quota Count must be greater than or equal to 0.");
            }
            if (quotaOptions.RuleType != LimitRuleType.FixedWindow && quotaOptions.Count < 1)
            {
                throw new NotSupportedException("Quota Count must be greater than or equal to 1.");
            }
            if (quotaOptions.Locker != null && (quotaOptions.Locker.LockMillSeconds <= 0 || quotaOptions.Locker.WaitMillSeconds <= 0))
            {
                throw new NotSupportedException("Quota Locker LockMillSeconds and WaitMillSeconds must be greater than 0.");
            }
            var rate = ResolveRate(quotaOptions.Period);
            return new QuotaConfig
            {
                Count = quotaOptions.Count,
                Locker = quotaOptions.Locker ?? new LockerConfig(),
                PeriodCount = rate.Item2,
                PeriodTimeSpan = rate.Item1,
                RuleType = quotaOptions.RuleType
            };
        }

        protected virtual ValueTuple<TimeSpan, int> ResolveRate(string period)
        {
            if (string.IsNullOrWhiteSpace(period))
            {
                throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
            }
            var array = period.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (array.Length != 2)
            {
                throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
            }
            if (!int.TryParse(array[0].Trim(), out int periodCount))
            {
                throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
            }
            var periodTimeSpanStr = array[1].Trim().ToLower();
            TimeSpan periodTimeSpan;
            if (periodTimeSpanStr.EndsWith("ms"))
            {
                if (!int.TryParse(GetTimeValue(periodTimeSpanStr.Replace("ms", "")), out int periodTimeSpanCount))
                {
                    throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
                }
                periodTimeSpan = TimeSpan.FromMilliseconds(periodTimeSpanCount);
            }
            else
            {
                if (periodTimeSpanStr.Length < 1)
                {
                    throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
                }
                var multiple = periodTimeSpanStr.Last() switch
                {
                    's' => 1000,
                    'm' => 1000 * 60,
                    'h' => 1000 * 60 * 60,
                    'd' => 1000 * 60 * 60 * 24,
                    _ => throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d."),
                };
                if (!int.TryParse(periodTimeSpanStr.Length == 1 ? "1" : periodTimeSpanStr.Substring(0, periodTimeSpanStr.Length - 1), out int periodTimeSpanCount))
                {
                    throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
                }
                periodTimeSpan = TimeSpan.FromMilliseconds(multiple * periodTimeSpanCount);
            }
            if (periodCount <= 0 || periodTimeSpan.TotalMilliseconds <= 0)
            {
                throw new NotSupportedException("the period must like 100/2ms 1/1s 5/2m 3/1h 5/1d.");
            }
            return (periodTimeSpan, periodCount);
        }

        private string GetTimeValue(string timeValue)
        {
            if (string.IsNullOrWhiteSpace(timeValue))
            {
                return "1";
            }
            return timeValue;
        }
    }
}