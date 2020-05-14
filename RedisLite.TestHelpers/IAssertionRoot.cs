using System;

namespace RedisLite.TestHelpers
{
    public interface IAssertionRoot
    {
        void IsSuccess(string reason = null);
        void ThrewException(string reason = null);
        void ThrewException<TException>(string reason = null) where TException : Exception;
    }

    public interface IAssertionRoot<out TResult> : IAssertionRoot
    {
        IAssertionRoot<TResult> Validate(Action<TResult> validationAction);
    }
}