using DonetSchool.QoS.Config;

namespace DonetSchool.QoS.Rules
{
    public interface IRuleMatcher
    {
        bool IsMatched(RequestInfo requestInfo, out QoSConfig qoSConfig);
    }
}