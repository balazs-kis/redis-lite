namespace RedisLite.Client.Contracts
{
    public class SslOptions
    {
        private SslOptions(bool useSsl, string serverName)
        {
            UseSsl = useSsl;
            ServerName = serverName;
        }

        public bool UseSsl { get; }
        public string ServerName { get; }

        public static SslOptions NoSsl() =>
            new SslOptions(false, null);

        public static SslOptions UseSslWithServerName(string serverName) =>
            new SslOptions(true, serverName);
    }
}