using IntegrationGateway.Application.Abstractions;
using IntegrationGateway.Application.Dtos;
using IntegrationGateway.Domain.Entities;
using IntegrationGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Infrastructure.Services;

public sealed class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db) => _db = db;

    public async Task<object> CreateInvoiceAsync(InvoiceInDto dto, CancellationToken ct)
    {
        // Duplicate protection by ExternalInvoiceId (business idempotency)
        var existing = await _db.Invoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalInvoiceId == dto.ExternalInvoiceId, ct);

        if (existing is not null)
        {
            return new
            {
                id = existing.Id,
                externalInvoiceId = existing.ExternalInvoiceId,
                status = existing.Status.ToString()
            };
        }

        var invoiceDate = DateOnly.ParseExact(dto.InvoiceDate, "yyyy-MM-dd");

        var entity = new Invoice
        {
            ExternalInvoiceId = dto.ExternalInvoiceId,
            InvoiceDate = invoiceDate,
            CustomerExternalId = dto.Customer.ExternalCustomerId,
            CustomerName = dto.Customer.Name,
            Status = InvoiceStatus.Pending,
            Lines = dto.Lines.Select(l => new InvoiceLine
            {
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        };

        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new
        {
            id = entity.Id,
            externalInvoiceId = entity.ExternalInvoiceId,
            status = entity.Status.ToString()
        };
    }

    public async Task<object?> GetInvoiceAsync(Guid id, CancellationToken ct)
    {
        var inv = await _db.Invoices.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (inv is null) return null;

        return new
        {
            id = inv.Id,
            externalInvoiceId = inv.ExternalInvoiceId,
            invoiceDate = inv.InvoiceDate.ToString("yyyy-MM-dd"),
            customer = new { externalCustomerId = inv.CustomerExternalId, name = inv.CustomerName },
            status = inv.Status.ToString(),
            sendAttempts = inv.SendAttempts,
            lastError = inv.LastError,
            lines = inv.Lines.Select(l => new { l.Description, l.Quantity, l.UnitPrice })
        };
    }
}
