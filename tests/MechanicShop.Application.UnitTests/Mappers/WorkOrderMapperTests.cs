using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.workOrders.Enums;

namespace MechanicShop.Application.UnitTests.Mappers;

public class WorkOrderMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllFieldsNavigationsAndTotals()
    {
        var part = new PartBuilder().WithId(Guid.NewGuid()).WithPrice(100m).WithQuantity(2).Build();
        var repairTask = new RepairTaskBuilder().WithLaborCost(150m).WithParts(part).Build();

        var workOrder = new WorkOrderBuilder()
            .WithSpot(Spot.C)
            .WithState(WorkOrderState.Scheduled)
            .WithRepairTasks(repairTask)
            .Build();

        var vehicle = new CarBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        var invoice = new InvoiceBuilder().Build();

        workOrder.Car = vehicle;
        workOrder.Labor = labor;
        workOrder.Invoice = invoice;

        var totalPartsCost = part.Price * part.Quantity;   // 100 * 2 = 200
        var totalLaborCost = repairTask.LaborCost;          // 150
        var totalCost = totalPartsCost + totalLaborCost;    // 350
        var duration = (int)repairTask.EstimatedDurationInMins;

        var dto = workOrder.ToDto();

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(workOrder.State, dto.State);
        Assert.Equal(workOrder.CreatedAtUtc, dto.CreatedAt);

        Assert.NotNull(dto.Labor);
        Assert.Equal(workOrder.LaborId, dto.Labor!.LaborId);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", dto.Labor.Name);

        Assert.NotNull(dto.Vehicle);
        Assert.Equal(vehicle.Id, dto.Vehicle!.Id);
        Assert.Equal(vehicle.Make, dto.Vehicle.Make);
        Assert.Equal(vehicle.Model, dto.Vehicle.Model);
        Assert.Equal(vehicle.Year, dto.Vehicle.Year);
        Assert.Equal(vehicle.LicensePlate, dto.Vehicle.LicensePlate);

        Assert.Single(dto.RepairTasks);
        Assert.Equal(totalPartsCost, dto.TotalPartCost);
        Assert.Equal(totalLaborCost, dto.TotalLaborCost);
        Assert.Equal(totalCost, dto.TotalCost);
        Assert.Equal(duration, dto.TotalDurationInMins);
        Assert.Equal(invoice.Id, dto.InvoiceId);
    }

    [Fact]
    public void ToDto_WhenNavigationsUnset_LeavesVehicleLaborAndInvoiceNull()
    {
        var workOrder = new WorkOrderBuilder().Build();

        var dto = workOrder.ToDto();

        Assert.Null(dto.Vehicle);
        Assert.Null(dto.Labor);
        Assert.Null(dto.InvoiceId);
    }

    [Fact]
    public void ToDtos_ShouldMapListCorrectly()
    {
        var repairTask = new RepairTaskBuilder()
            .WithLaborCost(100m)
            .WithParts(new PartBuilder().WithId(Guid.NewGuid()).WithPrice(50m).WithQuantity(1).Build())
            .Build();

        var workOrder = new WorkOrderBuilder().WithRepairTasks(repairTask).Build();
        workOrder.Car = new CarBuilder().Build();
        workOrder.Labor = new EmployeeBuilder().Build();

        var dtos = new List<WorkOrder> { workOrder }.ToDtos();

        Assert.Single(dtos);
        var dto = dtos[0];
        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.State, dto.State);
        Assert.NotNull(dto.Labor);
        Assert.Equal(workOrder.LaborId, dto.Labor!.LaborId);
        Assert.NotNull(dto.Vehicle);
        Assert.Single(dto.RepairTasks);
    }

    [Fact]
    public void ToListItemDto_ShouldMapSummaryCorrectly()
    {
        var repairTask = new RepairTaskBuilder().WithName("Oil Change").Build();

        var workOrder = new WorkOrderBuilder().WithRepairTasks(repairTask).Build();
        var vehicle = new CarBuilder().Build();
        var labor = new EmployeeBuilder().Build();
        workOrder.Car = vehicle;
        workOrder.Labor = labor;

        var dto = workOrder.ToListItemDto();

        Assert.Equal(workOrder.Id, dto.WorkOrderId);
        Assert.Equal(workOrder.Spot, dto.Spot);
        Assert.Equal(workOrder.StartAtUtc, dto.StartAtUtc);
        Assert.Equal(workOrder.EndAtUtc, dto.EndAtUtc);
        Assert.Equal(vehicle.Make, dto.car.Make);
        Assert.Equal($"{labor.FirstName} {labor.LastName}", dto.Labor);
        Assert.Single(dto.RepairTasks);
        Assert.Equal("Oil Change", dto.RepairTasks[0]);
        Assert.Equal(workOrder.State, dto.State);
    }

    [Fact]
    public void ToListItemDto_WhenNoLabor_LeavesLaborNull()
    {
        var workOrder = new WorkOrderBuilder().Build();
        workOrder.Car = new CarBuilder().Build();

        var dto = workOrder.ToListItemDto();

        Assert.Null(dto.Labor);
    }
}
