namespace Shared.Kernel;

public readonly record struct Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error Invariant(string message) => new("invariant.violation", message);
    public static Error Precondition(string message) => new("precondition.failed", message);
    public static Error NotFound(string message) => new("not.found", message);
    public static Error Conflict(string message) => new("conflict", message);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None) throw new InvalidOperationException();
        if (!isSuccess && error == Error.None) throw new InvalidOperationException();
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

public sealed class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("No value on failure result");

    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;
}
