using System;

namespace DonetSchool.Zk.Locks
{
    public class LockKeyErrorException : Exception
    {
        public LockKeyErrorException(string key) : base($"the key [{key}] is error.")
        {
        }
    }
}