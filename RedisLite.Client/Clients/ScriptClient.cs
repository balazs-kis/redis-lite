using System;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class ScriptClient : BaseClient
    {
        private const int ShaLength = 40;

        public Result<string> LoadScript(ISession session, string script)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SCRIPT_LOAD)
                        .WithParameter(script)
                        .ToString();

                var response = SendCommandAndReadResponse(session, command);
                var responseString = response[0]?.ToString();

                return responseString?.Length == ShaLength && !responseString.Contains(" ")
                    ? Result.Ok(responseString)
                    : Result.Fail<string>(responseString);
            }
            catch (Exception ex)
            {
                return Result.Fail<string>(ex.Message, ex);
            }
        }

        public Result<object[]> ExecuteScript(ISession session, string sha, string[] parameters)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.EVALSHA)
                        .WithKey(sha)
                        .WithParameter(0)
                        .WithParameters(parameters)
                        .ToString();

                var res = SendCommandAndReadResponse(session, command);
                
                return Result.Ok(res);
            }
            catch (Exception ex)
            {
                return Result.Fail<object[]>(ex.Message, ex);
            }
        }
    }
}