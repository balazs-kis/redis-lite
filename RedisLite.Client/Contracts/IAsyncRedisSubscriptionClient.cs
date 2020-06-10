using System;
using System.Threading.Tasks;

namespace RedisLite.Client.Contracts
{
    public interface IAsyncRedisSubscriptionClient : IDisposable
    {
        event Action<IAsyncRedisSubscriptionClient> OnConnected;
        event Action<string, string> OnMessageReceived;

        Task Connect(ConnectionSettings settings);

        Task Select(int dbIndex);

        Task Subscribe(params string[] channels);
        Task Unsubscribe(params string[] channels);
    }
}