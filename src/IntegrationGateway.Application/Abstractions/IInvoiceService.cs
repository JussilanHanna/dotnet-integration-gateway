using IntegrationGateway.Application.Dtos;

namespace IntegrationGateway.Application.Abstractions;

public interface IInvoiceService
{
    Task<object> CreateInvoiceAsync(InvoiceInDto dto, CancellationToken ct);
    Task<object?> GetInvoiceAsync(Guid id, CancellationToken ct);
}
