namespace DonetSchool.Zk
{
    public interface IConfigProvider
    {
        ZkConfig GetConfig(string name);
    }
}