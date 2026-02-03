using IntegrationGateway.Domain.Entities;
using IntegrationGateway.Infrastructure.Data;
using IntegrationGateway.Infrastructure.Partner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationGateway.Infrastructure.Workers;

public sealed class InvoiceSenderWorker : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<InvoiceSenderWorker> _log;

    public InvoiceSenderWorker(IServiceProvider sp, ILogger<InvoiceSenderWorker> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // yksinkertainen pollaus 10s välein
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var partner = scope.ServiceProvider.GetRequiredService<IPartnerClient>();

                var pending = await db.Invoices
                    .Where(x => x.Status == InvoiceStatus.Pending && x.SendAttempts < 5)
                    .OrderBy(x => x.CreatedUtc)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var inv in pending)
                {
                    inv.SendAttempts++;

                    try
                    {
                        var payload = new
                        {
                            invoiceId = inv.Id,
                            externalInvoiceId = inv.ExternalInvoiceId,
                            invoiceDate = inv.InvoiceDate.ToString("yyyy-MM-dd"),
                            customer = new { inv.CustomerExternalId, inv.CustomerName },
                            lines = inv.Lines.Select(l => new { l.Description, l.Quantity, l.UnitPrice })
                        };

                        await partner.SendInvoiceAsync(payload, stoppingToken);

                        inv.Status = InvoiceStatus.Sent;
                        inv.LastError = null;

                        _log.LogInformation("Invoice sent: {InvoiceId} ext={ExternalId}", inv.Id, inv.ExternalInvoiceId);
                    }
                    catch (Exception ex)
                    {
                        inv.Status = InvoiceStatus.Pending; // pysyy pendinginä retryä varten
                        inv.LastError = ex.Message;

                        // jos haluat: 5. yrityksen jälkeen Failed
                        if (inv.SendAttempts >= 5)
                        {
                            inv.Status = InvoiceStatus.Failed;
                        }

                        _log.LogWarning(ex, "Invoice send failed: {InvoiceId} attempt={Attempt}", inv.Id, inv.SendAttempts);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Worker loop crashed");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
