using System;
using System.IO;
using System.Threading.Tasks;

namespace RedisLite.Client.Networking
{
    internal interface ISession : IDisposable
    {
        bool IsOpen { get; }
        Locker Locker { get; }
        StreamReader StreamReader { get; }
        StreamWriter StreamWriter { get; }

        Task<bool> OpenAsync(string address, int port, TimeSpan receiveTimeout);
        void SetInfiniteReadTimeout();
    }
}