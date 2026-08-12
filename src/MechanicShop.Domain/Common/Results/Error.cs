namespace MechanicShop.Domain.Common.Results
{
    public readonly record struct Error
    {
        public string Code { get; }
        public string Description { get; }
        public ErrorKind Type { get; }

        private Error(string code,string description, ErrorKind type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        public static Error Failure(string code = nameof(Failure), string description ="General failure.")
             => new Error(code, description, ErrorKind.failure);
       
       public static Error Unexpected(string code = nameof(Unexpected), string description = "An unexpected error occurred.")
            => new Error(code, description, ErrorKind.unexpected);

        public static Error NotFound(string code = nameof(NotFound), string description = "The requested resource was not found.")
            => new Error(code, description, ErrorKind.NotFound);

        public static Error Validation(string code = nameof(Validation), string description = "Validation failed for the request.")
            => new Error(code, description, ErrorKind.Validation);

        public static Error Conflict(string code = nameof(Conflict), string description = "A conflict occurred with the current state of the resource.")
            => new Error(code, description, ErrorKind.Conflict);

        public static Error Unauthorized(string code = nameof(Unauthorized), string description = "The request requires user authentication.")
            => new Error(code, description, ErrorKind.Unauthorized);

        public static Error Forbidden(string code = nameof(Forbidden), string description = "The server understood the request but refuses to authorize it.")
            => new Error(code, description, ErrorKind.Forbidden);

        public static Error InternalServerError(string code = nameof(InternalServerError), string description = "The server encountered an internal error and was unable to complete your request.")
            => new Error(code, description, ErrorKind.InternalServerError);
    }
}