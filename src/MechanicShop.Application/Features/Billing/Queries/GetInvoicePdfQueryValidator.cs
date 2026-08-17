using FluentValidation;

namespace MechanicShop.Application.Features.Billing.Queries;

public class GetInvoicePdfQueryValidator : AbstractValidator<GetInvoicePdfQuery>
{
    public GetInvoicePdfQueryValidator()
    {
        RuleFor(i => i.InvoiceId).NotEmpty().WithMessage("Invoice Id is Required");
    }
}