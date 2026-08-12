using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MechanicShop.Domain.Common.Results
{

    public readonly record struct Success;
    public readonly record struct Created;
    public readonly record struct Deleted;
    public readonly record struct Updated;


    public sealed record Result<TValue> : IResult<TValue>
    {
        private readonly TValue? _value = default;

        private readonly List<Error>? _errors = null;

        public bool IsSuccess {get;}
        public bool IsError => !IsSuccess;

        public List<Error>? Errors => IsError ? _errors! : [];

        public TValue Value => IsSuccess ? _value! : default!;

        public Error TopError => (_errors?.Count > 0) ? _errors[0] : default!;

        [JsonConstructor]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("For serializer only.", true)]
        public Result(TValue value, List<Error>? errors, bool isSuccess)
        {
            if (isSuccess)
            {
                _value = value ?? throw new ArgumentNullException(nameof(value));
                _errors = [];
                IsSuccess = true;
            }
            else
            {
                if(errors == null || errors.Count == 0)
                    throw new ArgumentException("Errors cannot be null or empty when isSuccess is false", nameof(errors));

                _value = default;
                _errors = errors;
                IsSuccess = false;
            }
        }

        public TNextValue Match<TNextValue>(Func<TValue, TNextValue> OnValue, Func<List<Error>, TNextValue> onError)
            => IsSuccess ? OnValue(Value!) : onError(Errors!);

        private Result(Error error)
        {
            _errors = new List<Error> { error };
            IsSuccess = false;
        }
        private Result(List<Error> errors)
        {
            if(errors == null || errors.Count == 0)
                throw new ArgumentException("Errors cannot be null or empty", nameof(errors));
            
            _errors = errors;
            IsSuccess = false;
        }
        private Result(TValue value)
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value), "Value cannot be null");

            _value = value;
            IsSuccess = true;
        }

        public static implicit operator Result<TValue>(TValue value) 
            => new (value);

        public static implicit operator Result<TValue>(Error error) 
            => new (error);

        public static implicit operator Result<TValue>(List<Error> errors) 
            => new (errors);
    }

   public static class Result
   {
     public static Success success => default;
     public static Created created => default;
     public static Deleted deleted => default;
     public static Updated updated => default;

   }
}