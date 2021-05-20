namespace DonetSchool.QoS.Config
{
    public class LockerConfig
    {
        public LockerConfig()
        {
            WaitMillSeconds = 5_000;
            LockMillSeconds = 5_000;
        }

        /// <summary>
        /// 等待获取锁时间 毫秒
        /// </summary>
        public int WaitMillSeconds { get; set; }

        /// <summary>
        /// 获取锁之后锁时长 毫秒
        /// </summary>
        public int LockMillSeconds { get; set; }
    }
}