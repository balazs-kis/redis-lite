using System;
using System.Collections.Generic;
using System.Linq;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class SetClient : BaseClient
    {
        public Result<long> SAdd(ISession session, string key, params string[] members)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SADD)
                        .WithKey(key)
                        .WithParameters(members)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
                var responseString = response[0]?.ToString();

                var isOk = long.TryParse(responseString, out var result);
                return isOk
                    ? Result.Ok(result)
                    : Result.Fail<long>(responseString);

            }
            catch (Exception ex)
            {
                return Result.Fail<long>(ex.Message, ex);
            }
        }

        public Result<long> SRem(ISession session, string key, params string[] members)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SREM)
                        .WithKey(key)
                        .WithParameters(members)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
                var responseString = response[0]?.ToString();

                var isOk = long.TryParse(responseString, out var result);
                return isOk
                    ? Result.Ok(result)
                    : Result.Fail<long>(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail<long>(ex.Message, ex);
            }
        }

        public Result<bool> SIsMember(ISession session, string key, string member)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SISMEMBER)
                        .WithKey(key)
                        .WithParameter(member)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
                var responseString = response[0]?.ToString();

                var isOk = int.TryParse(responseString, out var result);
                return isOk
                    ? Result.Ok(result == 1)
                    : Result.Fail<bool>(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.Message, ex);
            }
        }

        public Result<List<string>> SMembers(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SMEMBERS)
                        .WithKey(key)
                        .ToString();

                var result = SendCommandAndReadResponse(session, command);
                
                return Result.Ok(result.Select(i => i.ToString()).ToList());
            }
            catch (Exception ex)
            {
                return Result.Fail<List<string>>(ex.Message, ex);
            }
        }

        public Result<long> SCard(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SCARD)
                        .WithKey(key)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
                var responseString = response[0]?.ToString();

                var isOk = long.TryParse(responseString, out var result);
                return isOk
                    ? Result.Ok(result)
                    : Result.Fail<long>(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail<long>(ex.Message, ex);
            }
        }
    }
}