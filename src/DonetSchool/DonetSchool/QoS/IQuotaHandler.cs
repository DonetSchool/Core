using DonetSchool.QoS.Config;
using System.Threading.Tasks;

namespace DonetSchool.QoS
{
    public interface IQuotaHandler
    {
        LimitRuleType RuleType { get; }

        Task<(bool isAllow, int waittimeMills)> IsAllowRequestAsync(string requestIdentity, QuotaConfig config);
    }
}