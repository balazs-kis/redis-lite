using System;
using System.IO;

namespace RedisLite.Client.Networking
{
    internal interface ISession : IDisposable
    {
        bool IsOpen { get; }
        Locker Locker { get; }
        StreamReader StreamReader { get; }
        StreamWriter StreamWriter { get; }
        
        bool Open(string address, int port, TimeSpan receiveTimeout);
        void SetInfiniteReadTimeout();
    }
}