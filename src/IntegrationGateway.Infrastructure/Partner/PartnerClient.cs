using System.Net.Http.Json;

namespace IntegrationGateway.Infrastructure.Partner;

public interface IPartnerClient
{
    Task SendInvoiceAsync(object payload, CancellationToken ct);
}

public sealed class PartnerClient(HttpClient http) : IPartnerClient
{
    public async Task SendInvoiceAsync(object payload, CancellationToken ct)
    {
        var res = await http.PostAsJsonAsync("/partner/invoices", payload, ct);
        res.EnsureSuccessStatusCode();
    }
}
