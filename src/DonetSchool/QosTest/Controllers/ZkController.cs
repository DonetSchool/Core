using DonetSchool.Zk;
using DonetSchool.Zk.Listeners;
using DonetSchool.Zk.Locks;
using Microsoft.AspNetCore.Mvc;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QosTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ZkController : ControllerBase
    {
        private readonly IZkClientFactory _zkClientFactory;

        public ZkController(IZkClientFactory zkClientFactory)
        {
            _zkClientFactory = zkClientFactory;
        }

        [HttpGet("new")]
        public Task<string> CreateNew()
        {
            return _zkClientFactory.GetDefault().CreateAsync("/School/Test/New2", Guid.NewGuid().ToByteArray(), CreateMode.EPHEMERAL_SEQUENTIAL);
        }

        [HttpGet("get")]
        public List<string> Get()
        {
            return TestMemoryWatch.Childrens;
        }

        private static IDistributedLock _locker;

        [HttpGet("getlock")]
        public async Task<bool> GetLock()
        {
            var (IsSuccess, DistributedLock) = await _zkClientFactory.GetDefault().GetLockAsync("/School/lock", TimeSpan.FromSeconds(10));
            if (IsSuccess)
            {
                _locker = DistributedLock;
            }
            return IsSuccess;
        }

        [HttpGet("releaselock")]
        public bool ReleaseLock()
        {
            if (_locker != null)
            {
                _locker.Dispose();
                _locker = null;
                return true;
            }
            return false;
        }
    }

    public class TestMemoryWatch : BaseListener, IListener
    {
        public static List<string> Childrens = new List<string>();

        public TestMemoryWatch(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string[] WatchPaths => new string[] { "/School/Test" };

        public override async Task OnWatchNotice(ZkClient client, WatchedEvent @event)
        {
            Childrens = await client.GetChildrenListAsync("/School/Test");
        }
    }
}