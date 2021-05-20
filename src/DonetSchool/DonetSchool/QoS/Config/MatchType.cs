namespace DonetSchool.QoS.Config
{
    public enum MatchType
    {
        /// <summary>
        /// 对比相等 不区分大小写
        /// </summary>
        Equal = 1,

        /// <summary>
        /// 部分起始路径 部分起始路径  不区分大小写
        /// </summary>
        StartWith = 2,

        /// <summary>
        /// 取非 部分起始路径 部分起始路径 不区分大小写
        /// </summary>
        NotStartWith = 3,

        /// <summary>
        /// 正则
        /// </summary>
        Regex = 4,

        /// <summary>
        /// 取非 正则
        /// </summary>
        NotRegex = 5,

        /// <summary>
        /// 正则忽略大小写
        /// </summary>
        RegexIgnoreCase = 6,

        /// <summary>
        /// 取非 正则忽略大小写
        /// </summary>
        NotRegexIgnoreCase = 7
    }
}