using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;

namespace MechanicShop.Application.Features.Identity.Queries.GenerateTokens;

public sealed record GenerateTokenQuery(string UserName, string Password) : IRequest<Result<TokenResponse>>, ISensitiveRequest;