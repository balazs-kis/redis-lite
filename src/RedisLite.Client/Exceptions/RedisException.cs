using System;

namespace RedisLite.Client.Exceptions
{
    public class RedisException : Exception
    {
        public RedisException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}