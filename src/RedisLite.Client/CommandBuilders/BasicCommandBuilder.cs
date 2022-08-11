using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedisLite.Client.CommandBuilders
{
    internal class BasicCommandBuilder : CommandBuilder
    {
        private const string ParamSeparator = " ";

        private readonly List<string> _params;
        private string _key;

        public BasicCommandBuilder(RedisCommands command)
            : base(command)
        {
            _params = new List<string>();
        }

        public BasicCommandBuilder WithKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("The key cannot be null, empty or whitespace", nameof(key));
            }

            _key = key;
            return this;
        }

        public BasicCommandBuilder WithParameter(string parameter)
        {
            _params.Add(parameter);
            return this;
        }

        public BasicCommandBuilder WithParameters(IEnumerable<string> parameters)
        {
            _params.AddRange(parameters);
            return this;
        }

        public BasicCommandBuilder WithParameter(object parameter)
        {
            _params.Add(parameter?.ToString());
            return this;
        }

        public BasicCommandBuilder WithParameters(IEnumerable<object> parameters)
        {
            _params.AddRange(parameters.Select(o => o.ToString()));
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Command.ToString().Replace("_", " ") + ParamSeparator);

            if (!string.IsNullOrWhiteSpace(_key))
            {
                sb.Append($"\"{_key}\"");
            }

            _params.ForEach(p => sb.Append(ParamSeparator + $"\"{p}\""));

            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}