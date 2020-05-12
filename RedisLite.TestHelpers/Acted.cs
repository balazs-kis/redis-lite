using System;

namespace RedisLite.TestHelpers
{
    public sealed class Acted<T>
    {
        private readonly T _result;

        public Acted(T result)
        {
            _result = result;
        }

        public void Assert(Action<T> assertAction)
        {
            assertAction.Invoke(_result);
        }
    }
}