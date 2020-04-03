namespace RedisLite.Client.CommandBuilders
{
    internal abstract class CommandBuilder
    {
        protected readonly RedisCommands Command;

        protected CommandBuilder(RedisCommands command)
        {
            Command = command;
        }
    }
}