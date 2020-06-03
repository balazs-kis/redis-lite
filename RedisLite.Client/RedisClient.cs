using RedisLite.Client.Clients;
using RedisLite.Client.Contracts;
using RedisLite.Client.Exceptions;
using RedisLite.Client.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("RedisLite.Tests")]

namespace RedisLite.Client
{
    public sealed class RedisClient : IRedisClient
    {
        private ISession _session;

        private CommonClient _commonClient;
        private HashClient _hashClient;
        private ListClient _listClient;
        private SetClient _setClient;
        private ScriptClient _scriptClient;
        private StringClient _stringClient;
        private TransactionClient _transactionClient;
        private SubscriptionClient _subscriptionClient;


        public event Action<IRedisClient> OnConnected;


        public async Task Connect(ConnectionSettings settings)
        {
            if (_session != null)
            {
                throw new InvalidOperationException("Cannot connect because the client is already connected");
            }

            _commonClient = new CommonClient();
            _hashClient = new HashClient();
            _listClient = new ListClient();
            _scriptClient = new ScriptClient();
            _stringClient = new StringClient();
            _transactionClient = new TransactionClient();
            _setClient = new SetClient();
            _subscriptionClient = new SubscriptionClient();
            _session = await _commonClient.Connect(settings);

            if (!_session.IsOpen)
            {
                throw new IOException("Session could not be opened");
            }

            OnConnected?.Invoke(this);
        }


        public Task Select(int dbIndex) =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.Select(session, dbIndex);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while selecting DB '{dbIndex}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Ping() =>
            ExecuteWithSession(async session =>
            {
                var result = await _stringClient.Ping(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while PINGing redis server [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Set(string key, string value) =>
            ExecuteWithSession(async session =>
            {
                var result = await _stringClient.Set(session, key, value);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task<string> Get(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _stringClient.Get(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<bool> Exists(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.Exists(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<long> DbSize() =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.DbSize(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting size of the DB [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task Del(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.Del(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while deleting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task FlushDb(bool async = false) =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.FlushDb(session, async);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while flushing DB [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task SwapDb(int index1, int index2) =>
            ExecuteWithSession(async session =>
            {
                var result = await _commonClient.SwapDb(session, index1, index2);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while swapping DB #{index1} with #{index2} [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public Task HSet(string key, string field, string value) =>
            ExecuteWithSession(async session =>
            {
                var result = await _hashClient.HSet(session, key, field, value);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting hash to '{key}: {field} - {value}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task HMSet(string key, IDictionary<string, string> fieldValues) =>
            ExecuteWithSession(async session =>
            {
                var result = await _hashClient.HMSet(session, key, fieldValues);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting multiple hash values to '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task<string> HGet(string key, string field) =>
            ExecuteWithSession(async session =>
            {
                var result = await _hashClient.HGet(session, key, field);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting hash value from '{key} - {field}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<IEnumerable<string>> HMGet(string key, IEnumerable<string> fields) =>
            ExecuteWithSession(async session =>
            {
                var result = await _hashClient.HMGet(session, key, fields);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting multiple hash values from '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<IDictionary<string, string>> HGetAll(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _hashClient.HGetAll(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting all hash values from '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });


        public Task RPush(string key, params string[] values) =>
            ExecuteWithSession(async session =>
            {
                var result = await _listClient.RPush(session, key, values);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while pushing values to key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task<IEnumerable<string>> LRange(string key, int start, int stop) =>
            ExecuteWithSession(async session =>
            {
                var result = await _listClient.LRange(session, key, start, stop);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting values from key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value.AsEnumerable();
            });

        public Task<long> SAdd(string key, params string[] members) =>
            ExecuteWithSession(async session =>
            {
                var result = await _setClient.SAdd(session, key, members);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while adding values to set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<long> SRem(string key, params string[] members) =>
            ExecuteWithSession(async session =>
            {
                var result = await _setClient.SRem(session, key, members);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while removing values from set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<IEnumerable<string>> SMembers(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _setClient.SMembers(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting values from set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value.AsEnumerable();
            });

        public Task<bool> SIsMember(string key, string member) =>
            ExecuteWithSession(async session =>
            {
                var result = await _setClient.SIsMember(session, key, member);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while checking values existence in set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<long> SCard(string key) =>
            ExecuteWithSession(async session =>
            {
                var result = await _setClient.SCard(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting the cardinality of the set '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });


        public Task<string> LoadScript(string script) =>
            ExecuteWithSession(async session =>
            {
                var transformedScript = script.Replace("\r\n", " ").Replace("\n", " ").Replace("\"", "'");

                var result = await _scriptClient.LoadScript(session, transformedScript);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while loading script '{script.Substring(0, Math.Min(50, script.Length))}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public Task<IEnumerable<object>> EvalSha(string sha, string[] parameters) =>
            ExecuteWithSession(async session =>
            {
                var result = await _scriptClient.ExecuteScript(session, sha, parameters);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while executing script '{sha}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value.AsEnumerable();
            });


        public Task Watch(params string[] keys) =>
            ExecuteWithSession(async session =>
            {
                if (keys == null || !keys.Any() || keys.All(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        "Key list was null, empty or contained only invalid strings");
                }

                var result = await _transactionClient.Watch(session, keys);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while watching keys '{string.Join(", ", keys)}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Multi() =>
            ExecuteWithSession(async session =>
            {
                var result = await _transactionClient.Multi(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while starting transaction [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Exec() =>
            ExecuteWithSession(async session =>
            {
                var result = await _transactionClient.Exec(session);

                if (result.IsFailure)
                {
                    if (string.Equals(result.Error, RedisConstants.TransactionAborted))
                    {
                        throw new RedisMultiExecutionFailedException(
                            "Redis EXEC failed: the key was modified",
                            null);
                    }

                    throw new RedisException(
                        $"Error while executing transaction [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public Task Discard() =>
            ExecuteWithSession(async session =>
            {
                var result = await _transactionClient.Discard(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while discarding transaction [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public Task Publish(string channel, string message) =>
            ExecuteWithSession(async session =>
            {
                var result = await _subscriptionClient.Publish(session, channel, message);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while publishing message to the channel '{channel}' [REDIS CODE: {result.Error}]",
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
                if (_session != null && _session.IsOpen)
                {
                    _session.Dispose();
                }
            }
        }


        private Task ExecuteWithSession(Func<ISession, Task> asyncAction)
        {
            ValidateSession();
            return asyncAction.Invoke(_session);
        }

        private Task<T> ExecuteWithSession<T>(Func<ISession, Task<T>> asyncFunc)
        {
            ValidateSession();
            return asyncFunc.Invoke(_session);
        }

        private void ValidateSession()
        {
            if (_session == null)
            {
                throw new InvalidOperationException($"The '{nameof(Connect)}' method must be called before sending commands");
            }

            if (!_session.IsOpen)
            {
                throw new InvalidOperationException("The client is disconnected");
            }
        }
    }
}