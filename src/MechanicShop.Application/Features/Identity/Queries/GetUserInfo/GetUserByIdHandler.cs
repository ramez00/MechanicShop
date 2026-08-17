using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Identity.Dtos;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.Identity.Queries.GetUserInfo;

public class GetUserByIdHandler(
    ILogger<GetUserByIdHandler> logger, IIdentityService identityService
) : IRequestHandler<GetUserByIdQuery, Result<AppUserDto>>
{

    private readonly ILogger<GetUserByIdHandler> _logger = logger;
    private readonly IIdentityService _identityService = identityService;

    public async Task<Result<AppUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userResult = await _identityService.GetUserByIdAsync(request.userId);

        if(userResult.IsError)
            return userResult.Errors!;

        return userResult.Value;    
    }
}