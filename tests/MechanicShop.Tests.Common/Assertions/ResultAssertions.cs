using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Tests.Common.Assertions;

/// <summary>
/// Assertion helpers for the custom <see cref="Result{TValue}"/> so every test reads the same way.
/// </summary>
public static class ResultAssertions
{
    /// <summary>Asserts the result is a success and returns its value for further assertions.</summary>
    public static TValue ShouldBeSuccess<TValue>(this Result<TValue> result)
    {
        Assert.True(result.IsSuccess,
            $"Expected a successful result but it was an error: {DescribeErrors(result)}");
        return result.Value;
    }

    /// <summary>Asserts the result is an error (any).</summary>
    public static Error ShouldBeError<TValue>(this Result<TValue> result)
    {
        Assert.True(result.IsError, "Expected an error result but it was successful.");
        return result.TopError;
    }

    /// <summary>Asserts the result is an error whose top error equals <paramref name="expected"/>.</summary>
    public static void ShouldBeError<TValue>(this Result<TValue> result, Error expected)
    {
        Assert.True(result.IsError, "Expected an error result but it was successful.");
        Assert.Equal(expected.Code, result.TopError.Code);
        Assert.Equal(expected.Type, result.TopError.Type);
    }

    /// <summary>Asserts the result is an error with the given error code.</summary>
    public static void ShouldBeErrorWithCode<TValue>(this Result<TValue> result, string code)
    {
        Assert.True(result.IsError, "Expected an error result but it was successful.");
        Assert.Equal(code, result.TopError.Code);
    }

    /// <summary>Asserts the result is an error of the given <see cref="ErrorKind"/>.</summary>
    public static void ShouldBeErrorOfKind<TValue>(this Result<TValue> result, ErrorKind kind)
    {
        Assert.True(result.IsError, "Expected an error result but it was successful.");
        Assert.Equal(kind, result.TopError.Type);
    }

    private static string DescribeErrors<TValue>(Result<TValue> result)
        => result.Errors is { Count: > 0 }
            ? string.Join("; ", result.Errors.Select(e => $"{e.Code} ({e.Type})"))
            : "<no errors>";
}
