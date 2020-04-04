using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using RedisLite.Client.Clients;
using RedisLite.Client.Contracts;
using RedisLite.Client.Exceptions;
using RedisLite.Client.Networking;

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
            _session = _commonClient.Connect(settings);
            
            if (!_session.IsOpen)
            {
                throw new IOException("Session could not be opened");
            }

            OnConnected?.Invoke(this);
        }


        public void Select(int dbIndex)
        {
            var result = _commonClient.Select(_session, dbIndex);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while selecting DB '{dbIndex}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Ping()
        {
            var result = _stringClient.Ping(_session);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while PINGing redis server [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Set(string key, string value)
        {
            var result = _stringClient.Set(_session, key, value);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while setting key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public string Get(string key)
        {
            var result = _stringClient.Get(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public bool Exists(string key)
        {
            var result = _commonClient.Exists(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public long DbSize()
        {
            var result = _commonClient.DbSize(_session);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting size of the DB [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public void Del(string key)
        {
            var result = _commonClient.Del(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while deleting key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void FlushDb()
        {
            var result = _commonClient.FlushDb(_session);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while flushing DB [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void SwapDb(int index1, int index2)
        {
            var result = _commonClient.SwapDb(_session, index1, index2);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while swapping DB #{index1} with #{index2} [REDIS CODE: {result.Error}]", result.Exception);
            }
        }


        public void HSet(string key, string field, string value)
        {
            var result = _hashClient.HSet(_session, key, field, value);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while setting hash to '{key}: {field} - {value}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void HMSet(string key, IDictionary<string, string> fieldValues)
        {
            var result = _hashClient.HMSet(_session, key, fieldValues);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while setting multiple hash values to '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public string HGet(string key, string field)
        {
            var result = _hashClient.HGet(_session, key, field);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting hash value from '{key} - {field}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public IEnumerable<string> HMGet(string key, IEnumerable<string> fields)
        {
            var result = _hashClient.HMGet(_session, key, fields);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting multiple hash values from '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public IDictionary<string, string> HGetAll(string key)
        {
            var result = _hashClient.HGetAll(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting all hash values from '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }


        public void RPush(string key, params string[] values)
        {
            var result = _listClient.RPush(_session, key, values);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while pushing values to key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public IEnumerable<string> LRange(string key, int start, int stop)
        {
            var result = _listClient.LRange(_session, key, start, stop);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting values from key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public IEnumerable<string> SMembers(string key)
        {
            var result = _setClient.SMembers(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while getting values from key '{key}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }


        public string LoadScript(string script)
        {
            var transformedScript = script.Replace("\r\n", " ").Replace("\n", " ").Replace("\"", "'");

            var result = _scriptClient.LoadScript(_session, transformedScript);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while loading script '{script.Substring(0, Math.Min(50, script.Length))}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }

        public IEnumerable<object> EvalSha(string sha, string[] parameters)
        {
            var result = _scriptClient.ExecuteScript(_session, sha, parameters);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while executing script '{sha}' [REDIS CODE: {result.Error}]", result.Exception);
            }

            return result.Value;
        }


        public void Watch(string key)
        {
            var result = _transactionClient.Watch(_session, key);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while watching key '{string.Join(", ", key)}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Watch(string[] keys)
        {
            var result = _transactionClient.Watch(_session, keys);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while watching keys '{string.Join(", ", keys)}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Multi()
        {
            var result = _transactionClient.Multi(_session);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while starting transaction [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Exec()
        {
            var result = _transactionClient.Exec(_session);

            if (result.IsFailure)
            {
                if (string.Equals(result.Error, RedisConstants.TransactionAborted))
                {
                    throw new RedisMultiExecutionFailedException("Redis EXEC failed: the key was modified", null);
                }

                throw new RedisException($"Error while executing transaction [REDIS CODE: {result.Error}]", result.Exception);
            }
        }

        public void Discard()
        {
            var result = _transactionClient.Discard(_session);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while discarding transaction [REDIS CODE: {result.Error}]", result.Exception);
            }
        }
        

        public void Publish(string channel, string message)
        {
            if (_session == null || !_session.IsOpen)
            {
                throw new RedisException(
                    $"Error while publishing message to the channel '{channel}' [The connection is not open]",
                    new ObjectDisposedException(nameof(_session)));
            }

            var result = new SubscriptionClient().Publish(_session, channel, message);

            if (result.IsFailure)
            {
                throw new RedisException($"Error while publishing message to the channel '{channel}' [REDIS CODE: {result.Error}]", result.Exception);
            }
        }


        public void Dispose()
        {
            if (_session != null && _session.IsOpen)
            {
                _session.Dispose();
            }
        }
    }
}