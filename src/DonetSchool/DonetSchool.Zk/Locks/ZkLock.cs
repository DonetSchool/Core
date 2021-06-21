using System;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.Zk.Locks
{
    internal class ZkLock
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ZkLock(string dir, string lcokKey, TaskCompletionSource<(bool IsSuccess, IDistributedLock DistributedLock)> taskCompletionSource, CancellationTokenSource cancellationTokenSource)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                throw new ArgumentNullException(nameof(dir));
            }
            if (string.IsNullOrWhiteSpace(lcokKey))
            {
                throw new ArgumentNullException(nameof(lcokKey));
            }
            Dir = dir;
            NodeName = new ZLockNodeName(lcokKey);
            TaskCompletionSource = taskCompletionSource;
            _cancellationTokenSource = cancellationTokenSource;
        }

        public string Dir { get; }

        public ZLockNodeName NodeName { get; }

        public string CurrentOwner { get; set; }

        public TaskCompletionSource<(bool IsSuccess, IDistributedLock DistributedLock)> TaskCompletionSource { get; }

        public bool IsCancel => _cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested;

        public bool IsOwner
        {
            get
            {
                return NodeName != null && NodeName.Name != null && CurrentOwner != null && CurrentOwner.Equals(NodeName.Name);
            }
        }

        public void SetResult((bool IsSuccess, IDistributedLock DistributedLock) result)
        {
            _cancellationTokenSource?.Dispose();
            TaskCompletionSource.SetResult(result);
        }
    }
}