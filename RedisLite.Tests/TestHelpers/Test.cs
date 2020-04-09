using System;

namespace RedisLite.Tests.TestHelpers
{
    internal static class Test
    {
        public static Arranged<T> Arrange<T>(Func<T> arrangeAction)
        {
            var underTest = arrangeAction.Invoke();
            return new Arranged<T>(underTest);
        }

        public static Arranged ArrangeNotNeeded()
        {
            return new Arranged();
        }

        public static ExecutionResult ForException(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                return ExecutionResult.CompletedWithException(ex);
            }

            return ExecutionResult.CompletedWithoutException();
        }
    }
}