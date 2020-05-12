using System;

namespace RedisLite.TestHelpers
{
    public static class Test
    {
        public static Arranged ArrangeNotNeeded()
        {
            return new Arranged();
        }

        public static Arranged<T> Arrange<T>(Func<T> arrangeFunc)
        {
            var underTest = arrangeFunc.Invoke();
            return new Arranged<T>(underTest);
        }
        
        public static Arranged<T, TParameter> Arrange<T, TParameter>(Func<(T, TParameter)> arrangeFunc)
        {
            var (underTest, parameter) = arrangeFunc.Invoke();
            return new Arranged<T, TParameter>(underTest, parameter);
        }

        public static Arranged<T, TParameter1, TParameter2> Arrange<T, TParameter1, TParameter2>(
            Func<(T, TParameter1, TParameter2)> arrangeFunc)
        {
            var (underTest, parameter1, parameter2) = arrangeFunc.Invoke();
            return new Arranged<T, TParameter1, TParameter2>(underTest, parameter1, parameter2);
        }
    }
}