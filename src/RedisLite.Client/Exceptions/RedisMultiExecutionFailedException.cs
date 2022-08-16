using System;

namespace RedisLite.Client.Exceptions
{
    public class RedisMultiExecutionFailedException : Exception
    {
        public RedisMultiExecutionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}