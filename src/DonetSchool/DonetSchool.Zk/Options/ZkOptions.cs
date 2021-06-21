namespace DonetSchool.Zk.Options
{
    public class ZkOptions
    {
        public ZkOptions()
        {
            SectionName = "ZK";
            DefaultName = "Main";
        }

        public string SectionName { get; set; }

        public string DefaultName { get; set; }
    }
}