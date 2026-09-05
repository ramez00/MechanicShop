namespace MechanicShop.Api.Controllers;

[Route("api/v{version:apiVersion}/invoices")]
[ApiVersion("1.0")]
[Authorize(Policy = "ManagerOnly")]
public sealed class InvoicesController(ISender sender) : ApiController
{
    [HttpGet("{invoiceId:guid}")]
    [Authorize(Policy = "ManagerOnly")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetInvoiceByIdQuery(invoiceId), cancellationToken);

        return result.Match(
            response => Ok(response),
            Problem);
    }


    [HttpPost("workorders/{workOrderId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> IssueInvoice(Guid workOrderId, CancellationToken cancellationToken)
    {
        var command = new IssueInvoiceCommand(workOrderId);
        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtAction(nameof(GetInvoice), new { version = "1.0", invoiceId = response.InvoiceId }, response),
            Problem);
    }

    [HttpGet("{invoiceId:guid}/pdf")]
    [Authorize(Policy = "ManagerOnly")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetInvoicePdf(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetInvoicePdfQuery(invoiceId), cancellationToken);

        return result.Match(
            response => File(response.Content!, "application/pdf", $"Invoice_{invoiceId}.pdf"),
            Problem);
    }

    [HttpPut("{invoiceId:guid}/payments")]
    [Authorize(Policy = "ManagerOnly")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> SettleInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        var command = new SettleInvoiceCommand(invoiceId);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}