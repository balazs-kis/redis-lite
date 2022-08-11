// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace RedisLite.Client.CommandBuilders
{
    internal enum RedisCommands
    {
        AUTH,
        DBSIZE,
        DEL,
        DISCARD,
        EVALSHA,
        EXEC,
        EXISTS,
        FLUSHDB,
        GET,
        HGET,
        HGETALL,
        HMGET,
        HMSET,
        HSET,
        KEYS,
        LRANGE,
        MULTI,
        PING,
        PUBLISH,
        RPUSH,
        SADD,
        SCARD,
        SCRIPT_LOAD,
        SELECT,
        SET,
        SISMEMBER,
        SMEMBERS,
        SREM,
        SUBSCRIBE,
        SWAPDB,
        UNSUBSCRIBE,
        WATCH
    }
}