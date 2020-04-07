using RedisLite.Client.Clients;
using RedisLite.Client.Contracts;
using RedisLite.Client.Exceptions;
using RedisLite.Client.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

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


        public void Connect(ConnectionSettings settings)
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
            _session = _commonClient.Connect(settings);

            if (!_session.IsOpen)
            {
                throw new IOException("Session could not be opened");
            }

            OnConnected?.Invoke(this);
        }


        public void Select(int dbIndex) =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.Select(session, dbIndex);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while selecting DB '{dbIndex}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void Ping() =>
            ExecuteWithSession(session =>
            {
                var result = _stringClient.Ping(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while PINGing redis server [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void Set(string key, string value) =>
            ExecuteWithSession(session =>
            {
                var result = _stringClient.Set(session, key, value);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public string Get(string key) =>
            ExecuteWithSession(session =>
            {
                var result = _stringClient.Get(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public bool Exists(string key) =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.Exists(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public long DbSize() =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.DbSize(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting size of the DB [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public void Del(string key) =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.Del(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while deleting key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void FlushDb(bool async = false) =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.FlushDb(session, async);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while flushing DB [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void SwapDb(int index1, int index2) =>
            ExecuteWithSession(session =>
            {
                var result = _commonClient.SwapDb(session, index1, index2);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while swapping DB #{index1} with #{index2} [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public void HSet(string key, string field, string value) =>
            ExecuteWithSession(session =>
            {
                var result = _hashClient.HSet(session, key, field, value);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting hash to '{key}: {field} - {value}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void HMSet(string key, IDictionary<string, string> fieldValues) =>
            ExecuteWithSession(session =>
            {
                var result = _hashClient.HMSet(session, key, fieldValues);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while setting multiple hash values to '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public string HGet(string key, string field) =>
            ExecuteWithSession(session =>
            {
                var result = _hashClient.HGet(session, key, field);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting hash value from '{key} - {field}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public IEnumerable<string> HMGet(string key, IEnumerable<string> fields) =>
            ExecuteWithSession(session =>
            {
                var result = _hashClient.HMGet(session, key, fields);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting multiple hash values from '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public IDictionary<string, string> HGetAll(string key) =>
            ExecuteWithSession(session =>
            {
                var result = _hashClient.HGetAll(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting all hash values from '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });


        public void RPush(string key, params string[] values) =>
            ExecuteWithSession(session =>
            {
                var result = _listClient.RPush(session, key, values);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while pushing values to key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public IEnumerable<string> LRange(string key, int start, int stop) =>
            ExecuteWithSession(session =>
            {
                var result = _listClient.LRange(session, key, start, stop);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting values from key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public long SAdd(string key, params string[] members) =>
            ExecuteWithSession(session =>
            {
                var result = _setClient.SAdd(session, key, members);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while adding values to set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public long SRem(string key, params string[] members) =>
            ExecuteWithSession(session =>
            {
                var result = _setClient.SRem(session, key, members);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while removing values from set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public IEnumerable<string> SMembers(string key) =>
            ExecuteWithSession(session =>
            {
                var result = _setClient.SMembers(session, key);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while getting values from set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public bool SIsMember(string key, string member) =>
            ExecuteWithSession(session =>
            {
                var result = _setClient.SIsMember(session, key, member);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while checking values existence in set key '{key}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });


        public string LoadScript(string script) =>
            ExecuteWithSession(session =>
            {
                var transformedScript = script.Replace("\r\n", " ").Replace("\n", " ").Replace("\"", "'");

                var result = _scriptClient.LoadScript(session, transformedScript);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while loading script '{script.Substring(0, Math.Min(50, script.Length))}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });

        public IEnumerable<object> EvalSha(string sha, string[] parameters) =>
            ExecuteWithSession(session =>
            {
                var result = _scriptClient.ExecuteScript(session, sha, parameters);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while executing script '{sha}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }

                return result.Value;
            });


        public void Watch(params string[] keys) =>
            ExecuteWithSession(session =>
            {
                if (keys == null || !keys.Any() || keys.All(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        "Key list was null, empty or contained only invalid strings");
                }

                var result = _transactionClient.Watch(session, keys);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while watching keys '{string.Join(", ", keys)}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void Multi() =>
            ExecuteWithSession(session =>
            {
                var result = _transactionClient.Multi(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while starting transaction [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });

        public void Exec() =>
            ExecuteWithSession(session =>
            {
                var result = _transactionClient.Exec(session);

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

        public void Discard() =>
            ExecuteWithSession(session =>
            {
                var result = _transactionClient.Discard(session);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while discarding transaction [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public void Publish(string channel, string message) =>
            ExecuteWithSession(session =>
            {
                var result = _subscriptionClient.Publish(session, channel, message);

                if (result.IsFailure)
                {
                    throw new RedisException(
                        $"Error while publishing message to the channel '{channel}' [REDIS CODE: {result.Error}]",
                        result.Exception);
                }
            });


        public void Dispose()
        {
            if (_session != null && _session.IsOpen)
            {
                _session.Dispose();
            }
        }


        private void ExecuteWithSession(Action<ISession> action)
        {
            ExecuteWithSession(session =>
            {
                action.Invoke(session);
                return true;
            });
        }

        private T ExecuteWithSession<T>(Func<ISession, T> func)
        {
            if (_session == null)
            {
                throw new InvalidOperationException($"The '{nameof(Connect)}' method must be called before sending commands");
            }

            if (!_session.IsOpen)
            {
                throw new InvalidOperationException("The client is disconnected");
            }

            return func.Invoke(_session);
        }
    }
}