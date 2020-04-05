﻿using RedisLite.Client.Contracts;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class LocalHostDefaultPort
    {
        internal static ConnectionSettings AsConnectionSettings() => new ConnectionSettings("127.0.0.1", 6379);
    }
}