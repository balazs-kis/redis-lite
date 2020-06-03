using System;
using System.Threading.Tasks;

namespace RedisLite.Client.Contracts
{
    public interface IRedisSubscriptionClient : IDisposable
    {
        event Action<IRedisSubscriptionClient> OnConnected;
        event Action<string, string> OnMessageReceived;

        Task Connect(ConnectionSettings settings);

        Task Select(int dbIndex);

        Task Subscribe(params string[] channels);
        Task Unsubscribe(params string[] channels);
    }
}