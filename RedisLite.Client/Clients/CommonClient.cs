using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Contracts;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class CommonClient : BaseClient
    {
        public async Task<ISession> Connect(ConnectionSettings settings)
        {
            var session = new Session(settings.DisableParallelExecutionChecking);
            await session.OpenAsync(settings.Address, settings.Port, settings.ReceiveTimeout);

            if (!settings.Authenticate)
            {
                return session;
            }

            var res = await Auth(session, settings.Secret);
            if (res.IsFailure)
            {
                throw new AuthenticationException(
                    $"Could not authenticate with the Redis server. Response code: {res.Error}", res.Exception);
            }

            return session;
        }

        public async Task<Result> Del(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.DEL)
                        .WithKey(key)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
                var responseString = response[0]?.ToString();

                return int.TryParse(responseString, out _) ||
                       string.Equals(responseString, RedisConstants.QueuedResult)
                    ? Result.Ok()
                    : Result.Fail(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> FlushDb(ISession session, bool async)
        {
            try
            {
                var commandBuilder = new BasicCommandBuilder(RedisCommands.FLUSHDB);
                if (async) commandBuilder.WithParameter(RedisConstants.Async);
                var command = commandBuilder.ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);

                return IsResponseOk(response[0]) ? Result.Ok() : Result.Fail(response[0]?.ToString());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> Select(ISession session, int dbIndex)
        {
            if (dbIndex < 0 || dbIndex > 15)
            {
                return Result.Fail("The DB index must be between 0 and 15 (given: {dbIndex})");
            }

            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SELECT)
                        .WithParameter(dbIndex)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);

                return IsResponseOk(response[0]) ? Result.Ok() : Result.Fail(response[0]?.ToString());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result> SwapDb(ISession session, int index1, int index2)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SWAPDB)
                        .WithParameter(index1)
                        .WithParameter(index2)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);

                return IsResponseOk(response[0]) ? Result.Ok() : Result.Fail(response[0]?.ToString());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }

        public async Task<Result<long>> DbSize(ISession session)
        {
            try
            {
                var command = new BasicCommandBuilder(RedisCommands.DBSIZE).ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
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

        public async Task<Result<bool>> Exists(ISession session, string key)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.EXISTS)
                        .WithKey(key)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);
                var parsed = int.TryParse(result[0]?.ToString(), out var resultInt);

                if (parsed)
                {
                    switch (resultInt)
                    {
                        case 0:
                            return Result.Ok(false);
                        case 1:
                            return Result.Ok(true);
                        default:
                            return Result.Fail<bool>($"Exists expected 0 or 1, got {resultInt}");
                    }
                }

                return Result.Fail<bool>($"Could not parse the returned value '{result[0]}' to integer");
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.Message, ex);
            }
        }

        public async Task<Result<List<string>>> Keys(ISession session, string pattern)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.KEYS)
                        .WithKey(pattern)
                        .ToString();

                var result = await SendCommandAndReadResponseAsync(session, command);
                return Result.Ok(result.Select(i => i.ToString()).ToList());
            }
            catch (Exception ex)
            {
                return Result.Fail<List<string>>(ex.Message, ex);
            }
        }

        private async Task<Result> Auth(ISession session, string secret)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.AUTH)
                        .WithKey(secret)
                        .ToString();

                var resultCode = await SendCommandAndReadResponseAsync(session, command);

                return string.Equals(resultCode[0].ToString(), RedisConstants.OkResult) ? Result.Ok() : Result.Fail(resultCode[0].ToString());
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message, ex);
            }
        }
    }
}