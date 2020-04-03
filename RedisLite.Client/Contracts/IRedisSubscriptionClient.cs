using System;

namespace RedisLite.Client.Contracts
{
    public interface IRedisSubscriptionClient : IDisposable
    {
        event Action<IRedisSubscriptionClient> OnConnected;
        event Action<string, string> OnMessageReceived;

        void Connect(ConnectionSettings settings);

        void Select(int dbIndex);

        void Subscribe(string channel);
        void Unsubscribe(string channel);

        void Subscribe(string[] channels);
        void Unsubscribe(string[] channels);
    }
}