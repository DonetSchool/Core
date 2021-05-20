namespace DonetSchool.QoS.Config
{
    public enum LimitRuleType
    {
        FixedWindow = 1,
        SlidingWindow = 2,
        LeakyBucket = 3,
        TokenBucket = 4
    }
}