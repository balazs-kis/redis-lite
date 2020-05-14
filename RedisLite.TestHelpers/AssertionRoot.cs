using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedisLite.TestHelpers
{
    public class AssertionRoot : IAssertionRoot
    {
        private readonly ActResult _typelessResult;

        public AssertionRoot(ActResult result)
        {
            _typelessResult = result;
        }

        public void IsSuccess(string reason = null)
        {
            if (_typelessResult.IsFailure)
            {
                throw new AssertFailedException(reason);
            }
        }

        public void ThrewException(string reason = null)
        {
            if (_typelessResult.IsSuccess)
            {
                throw new AssertFailedException(reason);
            }
        }

        public void ThrewException<TException>(string reason = null) where TException : Exception
        {
            if (_typelessResult.IsSuccess ||
                _typelessResult.Exception == null ||
                _typelessResult.Exception.GetType() != typeof(TException) &&
                !_typelessResult.Exception.GetType().IsSubclassOf(typeof(TException)))
            {
                throw new AssertFailedException(reason);
            }
        }
    }

    public sealed class AssertionRoot<TResult> : AssertionRoot, IAssertionRoot<TResult>
    {
        private readonly ActResult<TResult> _typedResult;

        public AssertionRoot(ActResult<TResult> result) : base(result)
        {
            _typedResult = result;
        }

        public IAssertionRoot<TResult> Validate(Action<TResult> validationAction)
        {
            if (_typedResult.IsFailure)
            {
                throw new AssertFailedException("Cannot validate result value, because the result state is not 'Success'.");
            }

            validationAction.Invoke(_typedResult.Value);
            return this;
        }
    }
}