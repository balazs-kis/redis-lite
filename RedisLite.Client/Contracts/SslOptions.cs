namespace RedisLite.Client.Contracts
{
    public class SslOptions
    {
        public static readonly SslOptions Default = NoSsl();

        private SslOptions(bool useSsl, bool useDefaultServerName, string serverName)
        {
            UseSsl = useSsl;
            UseDefaultServerName = useDefaultServerName;
            ServerName = serverName;
        }

        public bool UseSsl { get; }
        public bool UseDefaultServerName { get; }
        public string ServerName { get; }

        public static SslOptions NoSsl() =>
            new SslOptions(false, false, null);

        public static SslOptions UseSslWithDefaultServerName() =>
            new SslOptions(true, true, null);

        public static SslOptions UseSslWithServerName(string serverName) =>
            new SslOptions(true, false, serverName);
    }
}