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

        public void Execute(Action action)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    action();
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
            else
            {
                throw new InvalidOperationException(ParallelErrorMessage);
            }
        }

        public T Execute<T>(Func<T> function)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    return function();
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }

            throw new InvalidOperationException(ParallelErrorMessage);
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