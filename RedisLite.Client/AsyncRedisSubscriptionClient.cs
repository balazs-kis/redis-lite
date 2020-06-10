using RedisLite.Client.Clients;
using RedisLite.Client.Contracts;
using RedisLite.Client.Exceptions;
using RedisLite.Client.Networking;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RedisLite.Client
{
    public sealed class AsyncRedisSubscriptionClient : IAsyncRedisSubscriptionClient
    {
        private bool _isSubscribed;
        private ISession _session;
        private CommonClient _commonClient;
        private SubscriptionClient _subscriptionClient;

        public event Action<IAsyncRedisSubscriptionClient> OnConnected;
        public event Action<string, string> OnMessageReceived;


        public async Task Connect(ConnectionSettings settings)
        {
            if (_session != null)
            {
                throw new InvalidOperationException(
                    "Cannot connect because the client is already connected");
            }

            _commonClient = new CommonClient();
            _subscriptionClient = new SubscriptionClient();

            _session = await _commonClient.Connect(settings).ConfigureAwait(false);

            if (!_session.IsOpen)
            {
                throw new IOException("Session could not be opened");
            }

            OnConnected?.Invoke(this);
        }

        public Task Select(int dbIndex) =>
            ExecuteWithSession(async session =>
            {
                if (_isSubscribed)
                {
                    throw new InvalidOperationException(
                        "Subscribe has already been called on this client");
                }

                var result = await _commonClient.Select(_session, dbIndex).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while selecting DB '{dbIndex}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Subscribe(params string[] channels) =>
            ExecuteWithSession(async session =>
            {
                if (channels == null || !channels.Any() || channels.All(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        "Channel list was null, empty or contained only invalid strings");
                }

                if (_isSubscribed)
                {
                    throw new InvalidOperationException(
                        "Subscribe has already been called on this client");
                }

                var result = await _subscriptionClient.Subscribe(_session, channels).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while subscribing to channels ({string.Join(", ", channels)}) [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                _subscriptionClient.OnMessageReceived += RelayMessage;
                _isSubscribed = true;
            });

        public Task Unsubscribe(params string[] channels) =>
            ExecuteWithSession(async session =>
            {
                if (channels == null || !channels.Any() || channels.All(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        "Channel list was null, empty or contained only invalid strings");
                }

                if (!_isSubscribed)
                {
                    throw new InvalidOperationException(
                        "Subscribe has not yet been called on this client");
                }

                var result = await _subscriptionClient.Unsubscribe(_session, channels).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while unsubscribing from channels ({string.Join(", ", channels)}) [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isSubscribed)
                {
                    _subscriptionClient.OnMessageReceived -= RelayMessage;
                }

                if (_session != null && _session.IsOpen)
                {
                    _session.Dispose();
                }
            }
        }


        private void RelayMessage(string channel, string message)
        {
            OnMessageReceived?.Invoke(channel, message);
        }

        private Task ExecuteWithSession(Func<ISession, Task> asyncAction)
        {
            if (_session == null)
            {
                throw new InvalidOperationException($"The '{nameof(Connect)}' method must be called before sending commands");
            }

            if (!_session.IsOpen)
            {
                throw new InvalidOperationException("The client is disconnected");
            }

            return asyncAction.Invoke(_session);
        }
    }
}