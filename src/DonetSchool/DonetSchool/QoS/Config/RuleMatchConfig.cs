namespace DonetSchool.QoS.Config
{
    public class RuleMatchConfig
    {
        public string Pattern { get; set; }

        public MatchType MatchType { get; set; }

        public bool IsAllMethod { get; set; }

        public string[] Methods { get; set; }
    }
}