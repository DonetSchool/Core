using DonetSchool.QoS.Config;
using DonetSchool.QoS.Options;
using System.Collections.Generic;

namespace DonetSchool.QoS.Rules
{
    public interface IRuleProvider
    {
        List<QoSConfig> GetConfigs();

        void InstallRules(Dictionary<string, QoSOptions> qoSOptions);
    }
}