using FluentValidation;
using MechanicShop.Application.Common.Behaviors;
using MediatR;

namespace MechanicShop.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    // A minimal request whose response is a Result<T> (the behavior constrains TResponse : IResult).
    public sealed record SampleCommand(string Name) : IRequest<Result<string>>;

    private sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
            => RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }

    private static RequestHandlerDelegate<Result<string>> Next(Result<string> response, Action onCalled)
        => _ => { onCalled(); return Task.FromResult(response); };

    [Fact]
    public async Task Handle_WhenNoValidator_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>(validator: null);
        var called = false;

        var result = await behavior.Handle(new SampleCommand("ok"), Next("value", () => called = true), CancellationToken.None);

        Assert.True(called);
        Assert.Equal("value", result.ShouldBeSuccess());
    }

    [Fact]
    public async Task Handle_WhenValid_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>(new SampleCommandValidator());
        var called = false;

        var result = await behavior.Handle(new SampleCommand("valid"), Next("value", () => called = true), CancellationToken.None);

        Assert.True(called);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_WhenInvalid_ShortCircuitsWithValidationErrors()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>(new SampleCommandValidator());
        var called = false;

        var result = await behavior.Handle(new SampleCommand(""), Next("value", () => called = true), CancellationToken.None);

        Assert.False(called); // next is never invoked when validation fails
        result.ShouldBeErrorOfKind(ErrorKind.Validation);
        // The behavior maps FluentValidation failures onto Error.Validation(code: PropertyName, description: ErrorMessage).
        Assert.Equal(nameof(SampleCommand.Name), result.TopError.Code);
        Assert.Equal("Name is required", result.TopError.Description);
    }

    [Fact]
    public async Task Handle_WhenMultipleRulesFail_ReturnsAllErrors()
    {
        var validator = new InlineValidator<SampleCommand>();
        validator.RuleFor(x => x.Name).NotEmpty().WithMessage("required");
        validator.RuleFor(x => x.Name).MinimumLength(3).WithMessage("too short");
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>(validator);

        var result = await behavior.Handle(new SampleCommand(""), Next("value", () => { }), CancellationToken.None);

        result.ShouldBeError();
        Assert.Equal(2, result.Errors!.Count);
    }
}
