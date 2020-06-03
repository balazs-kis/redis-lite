using System;
using System.Threading;

namespace RedisLite.Client.Networking
{
    internal sealed class Locker
    {
        private const string ParallelErrorMessage = "Parallel execution is not supported";

        private readonly object _lock;

        public Locker()
        {
            _lock = new object();
        }


        public void Obtain()
        {
            var entered = Monitor.TryEnter(_lock);

            if (!entered)
            {
                throw new InvalidOperationException(ParallelErrorMessage);
            }
        }

        public void Release()
        {
            if (Monitor.IsEntered(_lock))
            {
                Monitor.Exit(_lock);
            }
        }
    }
}