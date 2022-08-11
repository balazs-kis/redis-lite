using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class SubscriptionClient : BaseClient
    {
        private readonly AutoResetEvent _unsubEvent = new AutoResetEvent(false);

        public event Action<string, string> OnMessageReceived;

        public async Task<Result> Publish(ISession session, string channel, string message)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.PUBLISH)
                        .WithKey(channel)
                        .WithParameter(message)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return int.TryParse(responseString, out _) ? Result.Ok() : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> Subscribe(ISession session, IEnumerable<string> channels)
        {
            try
            {
                var chs = channels.ToList();

                var command =
                    new BasicCommandBuilder(RedisCommands.SUBSCRIBE)
                        .WithParameters(chs)
                        .ToString();

                session.Locker?.Obtain();
                await SendCommandAsync(session, command);

                var isSubscriptionOk = true;
                string[] subscriptionResult = { "empty" };

                foreach (var ch in chs)
                {
                    subscriptionResult = (await ParseToEndAsync(session)).Select(i => i.ToString()).ToArray();

                    isSubscriptionOk &= string.Equals(subscriptionResult[0], "subscribe") &&
                                        string.Equals(subscriptionResult[1], ch) &&
                                        int.TryParse(subscriptionResult[2], out _);
                }

                if (!isSubscriptionOk)
                {
                    session.Locker?.Release();
                    return Result.Fail(subscriptionResult[0]);
                }

                session.SetInfiniteReadTimeout();

                _ = Task.Run(async () =>
                {
                    while (session.IsOpen)
                    {
                        var channelsHash = new HashSet<string>(chs);

                        object[] received;

                        try
                        {
                            received = await ParseToEndAsync(session);
                        }
                        catch (IOException)
                        {
                            // Client might be closed while waiting to receive subscription message.
                            // In this case the loop will exit at the next iteration.
                            continue;
                        }

                        var type = received[0].ToString();

                        var isMessageOk = string.Equals(type, "message") &&
                                          channelsHash.Contains(received[1].ToString());

                        if (!isMessageOk)
                        {
                            if (string.Equals(type, "unsubscribe"))
                            {
                                _unsubEvent.Set();
                            }

                            continue;
                        }

                        OnMessageReceived?.Invoke(received[1].ToString(), received[2].ToString());
                    }
                    session.Locker?.Release();
                }).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                session.Locker?.Release();
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> Unsubscribe(ISession session, IEnumerable<string> channels)
        {
            try
            {
                var chs = channels.ToList();

                var command =
                    new BasicCommandBuilder(RedisCommands.UNSUBSCRIBE)
                        .WithParameters(chs)
                        .ToString();

                await SendCommandAsync(session, command);

                var isUnsubOk = true;
                foreach (var ch in chs)
                {
                    isUnsubOk &= _unsubEvent.WaitOne(TimeSpan.FromSeconds(3));
                }

                return isUnsubOk ? Result.Ok() : Result.Fail("Timeout expired before unsubscribe notification was received");
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }
    }
}