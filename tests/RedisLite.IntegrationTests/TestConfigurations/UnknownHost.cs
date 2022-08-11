using RedisLite.Client.Contracts;

namespace RedisLite.IntegrationTests.TestConfigurations
{
    internal static class UnknownHost
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("host.not.correct", 9999);
    }
}