namespace CriGes.SharedKernel;

public sealed record AppError(string Code, string Message)
{
    public static readonly AppError None = new(string.Empty, string.Empty);
}

public class Result
{
    protected Result(bool isSuccess, AppError error)
    {
        if (isSuccess && error != AppError.None)
        {
            throw new InvalidOperationException("A successful result cannot contain an error.");
        }

        if (!isSuccess && error == AppError.None)
        {
            throw new InvalidOperationException("A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public AppError Error { get; }

    public static Result Success()
    {
        return new Result(true, AppError.None);
    }

    public static Result Failure(AppError error)
    {
        return new Result(false, error);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Failure<T>(AppError error)
    {
        return new Result<T>(error);
    }
}

public sealed class Result<T> : Result
{
    private readonly T? value;

    internal Result(T value)
        : base(true, AppError.None)
    {
        this.value = value;
    }

    internal Result(AppError error)
        : base(false, error)
    {
    }

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("The value of a failed result cannot be accessed.");
}
