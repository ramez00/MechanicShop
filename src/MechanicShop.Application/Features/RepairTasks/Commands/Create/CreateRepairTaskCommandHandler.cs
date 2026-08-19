using MechanicShop.Application.Common.Errors;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Mappers;
using MechanicShop.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Features.RepairTasks.Commands.Create;

public class CreateRepairTaskCommandHandler(
    ILogger<CreateRepairTaskCommandHandler> logger,
    IAppDbContext context,
    HybridCache cache
) : IRequestHandler<CreateRepairTaskCommand, Result<RepairTaskDto>>
{
    private readonly ILogger<CreateRepairTaskCommandHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly HybridCache _cache = cache;

    public async Task<Result<RepairTaskDto>> Handle(CreateRepairTaskCommand request, CancellationToken ct)
    {
        var nameExist = await  _context.RepairTasks.AnyAsync(p => EF.Functions.Like(p.Name,request.Name),ct);

        if(nameExist)
            return RepairTaskErrors.DuplicateName;

        List<Part> usedParts = [];

        foreach(var p in request.Parts)
        {
            var createdPart = Part.Create(Guid.NewGuid(),p.Name,p.Cost,p.Quantity);

            if(createdPart.IsError)
                return createdPart.Errors!;

            usedParts.Add(createdPart.Value);    
        }

        var createdRepairTask = RepairTask.Create(
            Guid.NewGuid(),
            request.Name!,
            request.LaborCost,
            request.EstimatedDurationInMins!.Value,
            usedParts);

        if(createdRepairTask.IsError)
            return createdRepairTask.Errors!;

        _context.RepairTasks.Add(createdRepairTask.Value);
        await _context.SaveChangesAsync(ct);

        return createdRepairTask.Value.ToDto();

    }
}