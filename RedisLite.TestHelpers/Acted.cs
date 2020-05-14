namespace RedisLite.TestHelpers
{
    public sealed class Acted
    {
        private readonly ActResult _result;

        public Acted(ActResult result)
        {
            _result = result;
        }

        public IAssertionRoot Assert() => new AssertionRoot(_result);
    }
    
    public sealed class Acted<T>
    {
        private readonly ActResult<T> _result;

        public Acted(ActResult<T> result)
        {
            _result = result;
        }

        public IAssertionRoot<T> Assert() => new AssertionRoot<T>(_result);
    }
}