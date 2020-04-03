using RedisLite.Client.Contracts;

namespace RedisLite.Tests.Setup
{
    internal static class LocalHostDefaultPort
    {
        internal static ConnectionSettings ConnectionSettings { get; } = new ConnectionSettings("127.0.0.1", 6379);
    }
}