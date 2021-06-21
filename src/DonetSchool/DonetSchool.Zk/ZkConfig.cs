using System.Text;

namespace DonetSchool.Zk
{
    public class ZkConfig
    {
        /// <summary>
        /// 配置名字(唯一)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 等待ZooKeeper连接的时间。
        /// </summary>
        public int ConnectionTimeout { get; set; } = 5000;

        /// <summary>
        /// 执行ZooKeeper操作的重试等待时间。
        /// </summary>
        public int OperatingTimeout { get; set; } = 3000;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 10;

        /// <summary>
        /// zookeeper会话超时时间。
        /// </summary>
        public int SessionTimeout { get; set; } = 15000;

        /// <summary>
        /// 是否只读，默认为false。
        /// </summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// 会话Id。
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// 会话密码。
        /// </summary>
        public string SessionPasswd { get; set; }

        public byte[] SessionPasswdBytes
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SessionPasswd))
                {
                    return Encoding.UTF8.GetBytes(SessionPasswd);
                }
                return null;
            }
        }
    }
}