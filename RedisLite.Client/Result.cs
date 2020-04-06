using System;
using System.Text;

namespace RedisLite.Client
{
    public class Result
    {
        public bool IsSuccess { get; }
        
        public bool IsFailure => !IsSuccess;

        public string Error { get; }

        public Exception Exception { get; }


        protected Result(bool isSuccess, string error, Exception ex)
        {
            if (!isSuccess && string.IsNullOrWhiteSpace(error))
            {
                throw new InvalidOperationException("Failure indicated, but the error message was empty");
            }

            IsSuccess = isSuccess;
            Error = error;

            Exception = ex;
        }


        public static Result Fail(string message, Exception ex = null) =>
            new Result(false, message, ex);
        
        public static Result<T> Fail<T>(string message, Exception ex = null) =>
            new Result<T>(default, false, message, ex);
        
        public static Result Ok() =>
            new Result(true, null, null);

        public static Result<T> Ok<T>(T value) =>
            new Result<T>(value, true, null, null);

        public override string ToString()
        {
            var sb = new StringBuilder($"[Result: {(IsSuccess ? "OK" : "FAIL")}");

            if (IsFailure)
            {
                sb.Append($", Error: {Error}");
                sb.Append(Exception != null ? $", Exception: {Exception.GetType()} - {Exception.Message}]" : "]");
            }
            else
            {
                sb.Append("]");
            }

            return sb.ToString();
        }
    }

    public sealed class Result<T> : Result
    {
        private readonly T _value;

        public T Value =>
            IsSuccess ? _value : throw new InvalidOperationException();

        internal Result(T value, bool isSuccess, string error, Exception ex)
            : base(isSuccess, error, ex)
        {
            _value = value;
        }
    }
}