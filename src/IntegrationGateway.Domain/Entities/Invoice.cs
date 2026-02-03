namespace IntegrationGateway.Domain.Entities;

public enum InvoiceStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public sealed class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // “Ulkoisen järjestelmän” lasku-id (esim NetvisorKey/InvoiceNumber)
    public string ExternalInvoiceId { get; set; } = default!;

    public DateOnly InvoiceDate { get; set; }

    public string CustomerExternalId { get; set; } = default!;
    public string CustomerName { get; set; } = default!;

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public int SendAttempts { get; set; } = 0;
    public string? LastError { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public List<InvoiceLine> Lines { get; set; } = new();
}

public sealed class InvoiceLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Description { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
