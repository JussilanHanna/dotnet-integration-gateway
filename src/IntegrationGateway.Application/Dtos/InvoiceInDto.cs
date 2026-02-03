namespace IntegrationGateway.Application.Dtos;

public sealed record InvoiceInDto(
    string ExternalInvoiceId,
    CustomerDto Customer,
    string InvoiceDate, // pidetään stringinä sisääntulossa → validointi/parse hallitusti
    List<InvoiceLineDto> Lines
);

public sealed record CustomerDto(string ExternalCustomerId, string Name);

public sealed record InvoiceLineDto(string Description, decimal Quantity, decimal UnitPrice);
