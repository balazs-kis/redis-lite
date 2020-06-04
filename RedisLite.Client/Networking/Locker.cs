using System;

namespace RedisLite.Client.Networking
{
    internal sealed class Locker
    {
        private const string ParallelErrorMessage = "Parallel execution is not supported";
        private const string ReleaseErrorMessage = "Parallel execution error: release without obtaining";

        private readonly object _lock;

        private int _allowedExecution;

        public Locker()
        {
            _lock = new object();
            _allowedExecution = 1;
        }


        public void Obtain()
        {
            lock (_lock)
            {
                if (_allowedExecution == 0)
                {
                    throw new InvalidOperationException(ParallelErrorMessage);
                }

                _allowedExecution = 0;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                if (_allowedExecution == 1)
                {
                    throw new InvalidOperationException(ReleaseErrorMessage);
                }

                _allowedExecution = 1;
            }
        }
    }
}