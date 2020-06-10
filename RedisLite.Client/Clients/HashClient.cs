using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class HashClient : BaseClient
    {
        public async Task<Result> HSet(ISession session, string key, string field, string value)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.HSET)
                        .WithKey(key)
                        .WithParameter(field)
                        .WithParameter(value)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return IsHashSetResponseOk(responseString) ? Result.Ok() : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> HMSet(ISession session, string key, IDictionary<string, string> fieldValues)
        {
            try
            {
                var parameters = fieldValues.SelectMany(kvp => new [] {kvp.Key, kvp.Value}).ToArray();

                var command =
                    new BasicCommandBuilder(RedisCommands.HMSET)
                        .WithKey(key)
                        .WithParameters(parameters)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return IsResponseOk(responseString) ? Result.Ok() : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result<string>> HGet(ISession session, string key, string field)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.HGET)
                        .WithKey(key)
                        .WithParameter(field)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return Result.Ok(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail<string>(ex.Message, ex);
            }
        }

        public async Task<Result<IEnumerable<string>>> HMGet(ISession session, string key, IEnumerable<string> fields)
        {
            try
            {
                var f = fields.ToArray();

                var command =
                    new BasicCommandBuilder(RedisCommands.HMGET)
                        .WithKey(key)
                        .WithParameters(f)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);
                
                var stringResult = result.Select(i => i?.ToString()).ToArray();

                if (result.Length != f.Length)
                {
                    Result.Fail<IDictionary<string, string>>($"Received {result.Length} items instead of {f.Length}: [{string.Join(", ", stringResult)}]");
                }

                return Result.Ok(stringResult.AsEnumerable());
            }
            catch (Exception ex)
            {
                return Result.Fail<IEnumerable<string>>(ex.Message, ex);
            }
        }

        public async Task<Result<IDictionary<string, string>>> HGetAll(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.HGETALL)
                        .WithKey(key)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);
                
                if (result.Length % 2 == 1)
                {
                    Result.Fail<IDictionary<string, string>>($"Received an odd number of items: [{string.Join(", ", result.Select(i => i?.ToString()))}]");
                }

                IDictionary<string, string> d = new Dictionary<string, string>();
                for (var i = 0; i < result.Length; i+= 2)
                {
                    d[result[i].ToString()] = result[i + 1].ToString();
                }

                return Result.Ok(d);
            }
            catch (Exception ex)
            {
                return Result.Fail<IDictionary<string, string>>(ex.Message, ex);
            }
        }


        private static bool IsHashSetResponseOk(string response)
        {
            return string.Equals(response, "1", StringComparison.CurrentCultureIgnoreCase) ||
                   string.Equals(response, "0", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}