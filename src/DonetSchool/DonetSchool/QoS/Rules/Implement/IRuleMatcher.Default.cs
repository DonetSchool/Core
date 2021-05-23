using DonetSchool.QoS.Config;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DonetSchool.QoS.Rules.Implement
{
    public class RuleMatcher : IRuleMatcher
    {
        private readonly IRuleProvider _ruleProvider;

        public RuleMatcher(IRuleProvider ruleProvider)
        {
            _ruleProvider = ruleProvider;
        }

        public bool IsMatched(RequestInfo requestInfo, out QoSConfig qoSConfig)
        {
            qoSConfig = null;
            if (requestInfo == null)
            {
                return false;
            }
            var configs = _ruleProvider.GetConfigs();
            if (configs == null || !configs.Any())
            {
                return false;
            }
            foreach (var item in configs)
            {
                var state = GetMatchState(item.MatchConfig, requestInfo);
                if (state == 1)
                {
                    qoSConfig = item;
                    break;
                }
                else if (state > 1)
                {
                    qoSConfig = item;
                }
            }
            return qoSConfig != null;
        }

        private int GetMatchState(RuleMatchConfig config, RequestInfo requestInfo)
        {
            //先判断方法是否匹配
            if (!(config.IsAllMethod || (config.Methods != null && config.Methods.Contains(requestInfo.Method, StringComparer.CurrentCultureIgnoreCase))))
            {
                return -1;
            }
            if ((config.MatchType == MatchType.Equal || config.MatchType == MatchType.StartWith) && string.Equals(config.Pattern, requestInfo.MatchInput, StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }
            return config.MatchType switch
            {
                MatchType.StartWith => requestInfo.MatchInput.StartsWith(config.Pattern, StringComparison.CurrentCultureIgnoreCase) ? 2 : -1,
                MatchType.NotStartWith => !requestInfo.MatchInput.StartsWith(config.Pattern, StringComparison.CurrentCultureIgnoreCase) ? 2 : -1,
                MatchType.Regex => Regex.IsMatch(requestInfo.MatchInput, config.Pattern, RegexOptions.Compiled) ? 1 : -1,
                MatchType.NotRegex => !Regex.IsMatch(requestInfo.MatchInput, config.Pattern, RegexOptions.Compiled) ? 1 : -1,
                MatchType.RegexIgnoreCase => Regex.IsMatch(requestInfo.MatchInput, config.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase) ? 1 : -1,
                MatchType.NotRegexIgnoreCase => !Regex.IsMatch(requestInfo.MatchInput, config.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase) ? 1 : -1,
                MatchType.Equal => -1,
                _ => throw new NotSupportedException(),
            };
        }
    }
}