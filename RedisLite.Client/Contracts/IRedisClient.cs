using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable InconsistentNaming
namespace RedisLite.Client.Contracts
{
    public interface IRedisClient : IDisposable
    {
        event Action<IRedisClient> OnConnected;

        Task Connect(ConnectionSettings settings);

        Task Select(int dbIndex);

        Task Ping();

        Task Set(string key, string value);
        Task<string> Get(string key);
        Task<bool> Exists(string key);
        Task<long> DbSize();
        Task Del(string key);
        Task FlushDb(bool async = false);
        Task SwapDb(int index1, int index2);

        Task HSet(string key, string field, string value);
        Task HMSet(string key, IDictionary<string, string> fieldValues);
        Task<string> HGet(string key, string field);
        Task<IEnumerable<string>> HMGet(string key, IEnumerable<string> fields);
        Task<IDictionary<string, string>> HGetAll(string key);

        Task RPush(string key, params string[] values);
        Task<IEnumerable<string>> LRange(string key, int start, int stop);

        Task<long> SAdd(string key, params string[] members);
        Task<long> SRem(string key, params string[] members);
        Task<IEnumerable<string>> SMembers(string key);
        Task<bool> SIsMember(string key, string member);
        Task<long> SCard(string key);

        Task<string> LoadScript(string script);
        Task<IEnumerable<object>> EvalSha(string sha, string[] parameters);

        Task Watch(params string[] keys);
        Task Multi();
        Task Exec();
        Task Discard();

        Task Publish(string channel, string message);
    }
}