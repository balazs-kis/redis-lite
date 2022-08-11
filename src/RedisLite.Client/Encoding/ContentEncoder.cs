namespace RedisLite.Client.Encoding
{
    internal static class ContentEncoder
    {
        public static System.Text.Encoding Encoding { get; set; }

        static ContentEncoder()
        {
            Encoding = System.Text.Encoding.ASCII;
        }
    }
}