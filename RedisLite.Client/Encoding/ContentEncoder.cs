namespace RedisLite.Client.Encoding
{
    internal static class ContentEncoder
    {
        public static System.Text.Encoding Encoding { get; set; }

        static ContentEncoder()
        {
            Encoding = System.Text.Encoding.ASCII;
        }

        public static string Decode(byte[] content)
        {
            return Encoding.GetString(content);
        }

        public static byte[] Encode(string content)
        {
            return Encoding.GetBytes(content);
        }
    }
}