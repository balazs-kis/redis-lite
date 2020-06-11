using System;

namespace RedisLite.Client.Networking
{
    internal sealed class Locker
    {
        private const string ParallelErrorMessage = "Parallel execution is not supported";
        private const string ReleaseErrorMessage = "Parallel execution error: release without obtaining";

        private readonly object _lock;

        private bool _isFree;

        public Locker()
        {
            _lock = new object();
            _isFree = true;
        }


        public void Obtain()
        {
            lock (_lock)
            {
                if (!_isFree)
                {
                    throw new InvalidOperationException(ParallelErrorMessage);
                }

                _isFree = false;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                if (_isFree)
                {
                    throw new InvalidOperationException(ReleaseErrorMessage);
                }

                _isFree = true;
            }
        }
    }
}