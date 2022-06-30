namespace RedisLite.Client
{
    internal static class RedisConstants
    {
        public const char ArrayPrefix = '*';
        public const char BulkPrefix = '$';
        public const char IntegerPrefix = ':';
        public const char StringPrefix = '+';
        public const char ErrorPrefix = '-';

        public const string NullBulk = "$-1";
        public const string NullList = "*-1";
        public const string OkResult = "OK";
        public const string QueuedResult = "QUEUED";

        public const string TransactionAborted = "ABORTED";

        public const string Pong = "PONG";
        public const string Async = "ASYNC";
    }
}