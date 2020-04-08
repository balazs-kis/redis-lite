using System;

namespace RedisLite.Tests.TestHelpers
{
    internal class Arranged<T>
    {
        private readonly T _underTest;

        public Arranged(T underTest)
        {
            _underTest = underTest;
        }

        public Acted<TResult> Act<TResult>(Func<T, TResult> actFunc)
        {
            var result = actFunc.Invoke(_underTest);

            return new Acted<TResult>(result);
        }
    }
}