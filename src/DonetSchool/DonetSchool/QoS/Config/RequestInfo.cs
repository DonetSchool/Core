namespace DonetSchool.QoS.Config
{
    public class RequestInfo
    {
        /// <summary>
        /// 匹配地址
        /// </summary>
        public string MatchInput { get; set; }

        /// <summary>
        /// 方法类型
        /// </summary>
        public string Method { get; set; }

        public override string ToString()
        {
            return $"I:{MatchInput},M:{Method}";
        }
    }
}