using System;
using System.Threading.Tasks;
using RedisLite.Client.CommandBuilders;
using RedisLite.Client.Networking;

namespace RedisLite.Client.Clients
{
    internal sealed class ScriptClient : BaseClient
    {
        private const int ShaLength = 40;

        public async Task<Result<string>> LoadScript(ISession session, string script)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.SCRIPT_LOAD)
                        .WithParameter(script)
                        .ToString();

                var response = await SendCommandAndReadResponseAsync(session, command);
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

        public async Task<Result<object[]>> ExecuteScript(ISession session, string sha, string[] parameters)
        {
            try
            {
                var command =
                    new BasicCommandBuilder(RedisCommands.EVALSHA)
                        .WithKey(sha)
                        .WithParameter(0)
                        .WithParameters(parameters)
                        .ToString();

                var res = await SendCommandAndReadResponseAsync(session, command);
                
                return Result.Ok(res);
            }
            catch (Exception ex)
            {
                return Result.Fail<object[]>(ex.Message, ex);
            }
        }
    }
}