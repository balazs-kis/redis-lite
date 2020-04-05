// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace RedisLite.Client.CommandBuilders
{
    internal enum RedisCommands
    {
        GET,
        SET,
        EXISTS,
        DBSIZE,
        DEL,
        FLUSHDB,
        LRANGE,
        RPUSH,
        SCRIPT_LOAD,
        EVALSHA,
        SELECT,
        AUTH,
        WATCH,
        MULTI,
        EXEC,
        DISCARD,
        PUBLISH,
        SUBSCRIBE,
        UNSUBSCRIBE,
        HSET,
        HMSET,
        HGET,
        HMGET,
        HGETALL,
        PING,
        SADD,
        SREM,
        SISMEMBER,
        SMEMBERS,
        SWAPDB
    }
}