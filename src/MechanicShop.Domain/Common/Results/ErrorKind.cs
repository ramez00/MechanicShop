namespace MechanicShop.Domain.Common.Results
{
    public enum ErrorKind
    {
        failure,
        unexpected,
        NotFound,
        Validation,
        Conflict,
        Unauthorized,
        Forbidden,
        InternalServerError
    }
}