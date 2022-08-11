using RedisLite.Client.Contracts;

namespace RedisLite.IntegrationTests.TestConfigurations
{
    internal static class LocalHostPort7000
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("127.0.0.1", 7000);
    }
}