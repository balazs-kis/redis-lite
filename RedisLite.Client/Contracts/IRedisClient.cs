using System;
using System.Collections.Generic;

namespace RedisLite.Client.Contracts
{
    public interface IRedisClient : IDisposable
    {
        event Action<IRedisClient> OnConnected;

        void Connect(ConnectionSettings settings);

        void Select(int dbIndex);

        void Ping();

        void Set(string key, string value);
        string Get(string key);
        bool Exists(string key);
        long DbSize();
        void Del(string key);
        void FlushDb();
        void SwapDb(int index1, int index2);

        void HSet(string key, string field, string value);
        void HMSet(string key, IDictionary<string, string> fieldValues);
        string HGet(string key, string field);
        IEnumerable<string> HMGet(string key, IEnumerable<string> fields);
        IDictionary<string, string> HGetAll(string key);

        void RPush(string key, params string[] values);
        IEnumerable<string> LRange(string key, int start, int stop);

        string LoadScript(string script);
        IEnumerable<object> EvalSha(string sha, string[] parameters);

        void Watch(string key);
        void Watch(string[] keys);
        void Multi();
        void Exec();
        void Discard();

        void Publish(string channel, string message);
    }
}