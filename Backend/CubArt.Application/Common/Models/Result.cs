using CubArt.Domain.Exceptions;
using System.Text.Json.Serialization;

namespace CubArt.Application.Common.Models
{
    // Result pattern для обработки успехов/ошибок
    public record Result
    {
        public bool IsSuccess { get; init; }
        public string[] Errors { get; init; } = Array.Empty<string>();
        public string? ErrorMessage => Errors.Length > 0 ? string.Join(Environment.NewLine, Errors) : null;

        [JsonIgnore]
        public string? ExceptionMessage { get; init; }

        protected Result() { }

        [JsonConstructor]
        protected Result(bool isSuccess, string[] errors, string? exceptionMessage = null)
        {
            IsSuccess = isSuccess;
            Errors = errors ?? Array.Empty<string>();
            ExceptionMessage = exceptionMessage;
        }

        public static Result Success() => new() { IsSuccess = true };
        public static Result<T> Success<T>(T value) => new() { IsSuccess = true, Data = value };
        public static Result Failure(string error, Exception? exception = null) =>
            new() { IsSuccess = false, Errors = new[] { error }, ExceptionMessage = exception?.GetFullExceptionDetails() };
        public static Result Failure(string[] errors, Exception? exception = null) =>
            new() { IsSuccess = false, Errors = errors, ExceptionMessage = exception?.GetFullExceptionDetails() };
        public static Result<T> Failure<T>(string error, Exception? exception = null) =>
            new() { IsSuccess = false, Errors = new[] { error }, ExceptionMessage = exception?.GetFullExceptionDetails() };
        public static Result<T> Failure<T>(string[] errors, Exception? exception = null) =>
            new() { IsSuccess = false, Errors = errors, ExceptionMessage = exception?.GetFullExceptionDetails() };
    }

    public record Result<T> : Result
    {
        public T Data { get; init; } = default!;
    }
}