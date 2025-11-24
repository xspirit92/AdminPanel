using CubArt.Domain.Exceptions;
using System.Text.Json.Serialization;

namespace CubArt.Application.Common.Models
{
    // Result pattern для обработки успехов/ошибок
    public class Result
    {
        public bool IsSuccess { get; }
        private string[] Errors { get; }
        public string? ErrorMessage => Errors.Length > 0 ? string.Join(Environment.NewLine, Errors) : null;

        [JsonIgnore]
        public string? ExceptionMessage { get; }

        [JsonConstructor]
        protected Result(bool isSuccess, string[] errors, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? Array.Empty<string>();
            ExceptionMessage = exception?.GetFullExceptionDetails();
        }

        public static Result Success() => new Result(true, Array.Empty<string>());
        public static Result<T> Success<T>(T value) => new Result<T>(value, true, Array.Empty<string>());
        public static Result Failure(string error, Exception? exception = null) => new Result(false, new[] { error }, exception);
        public static Result Failure(string[] errors, Exception? exception = null) => new Result(false, errors, exception);
        public static Result<T> Failure<T>(string error, Exception? exception = null) => new Result<T>(default, false, new[] { error }, exception);
        public static Result<T> Failure<T>(string[] errors, Exception? exception = null) => new Result<T>(default, false, errors, exception);
    }

    public class Result<T> : Result
    {
        private readonly T _data;

        public T Data
        {
            get
            {
                //if (!IsSuccess)
                //    throw new InvalidOperationException("Cannot access Value of failed result");
                return _data;
            }
        }

        [JsonConstructor]
        protected internal Result(T data, bool isSuccess, string[] errors, Exception? exception = null) : base(isSuccess, errors, exception)
        {
            _data = data;
        }
    }
}
