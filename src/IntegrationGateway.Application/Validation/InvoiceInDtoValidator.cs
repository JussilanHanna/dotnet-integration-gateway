using FluentValidation;
using IntegrationGateway.Application.Dtos;

namespace IntegrationGateway.Application.Validation;

public sealed class InvoiceInDtoValidator : AbstractValidator<InvoiceInDto>
{
    public InvoiceInDtoValidator()
    {
        RuleFor(x => x.ExternalInvoiceId).NotEmpty().MaximumLength(64);

        RuleFor(x => x.Customer).NotNull();
        RuleFor(x => x.Customer.ExternalCustomerId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Customer.Name).NotEmpty().MaximumLength(256);

        RuleFor(x => x.InvoiceDate)
            .NotEmpty()
            .Must(BeValidDate)
            .WithMessage("InvoiceDate must be in format yyyy-MM-dd.");

        RuleFor(x => x.Lines).NotNull().NotEmpty();
        RuleForEach(x => x.Lines).SetValidator(new InvoiceLineDtoValidator());
    }

    private static bool BeValidDate(string s) =>
        DateOnly.TryParseExact(s, "yyyy-MM-dd", out _);
}

public sealed class InvoiceLineDtoValidator : AbstractValidator<InvoiceLineDto>
{
    public InvoiceLineDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
