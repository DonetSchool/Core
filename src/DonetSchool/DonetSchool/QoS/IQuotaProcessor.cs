using DonetSchool.QoS.Config;
using System.Threading.Tasks;

namespace DonetSchool.QoS
{
    public interface IQuotaProcessor
    {
        Task ProcessAsync(string requestIdentity, QuotaConfig quotaConfig);
    }
}