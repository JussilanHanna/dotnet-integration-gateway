using System.Text;
using System.Text.Json;
using IntegrationGateway.Domain.Entities;
using IntegrationGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntegrationGateway.Api.Idempotency;

public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, AppDbContext db)
    {
        // sovitaan header: Idempotency-Key
        if (!ctx.Request.Headers.TryGetValue("Idempotency-Key", out var keyValues))
        {
            await _next(ctx);
            return;
        }

        var key = keyValues.ToString().Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            await _next(ctx);
            return;
        }

        var route = $"{ctx.Request.Method} {ctx.Request.Path}".ToLowerInvariant();

        var existing = await db.IdempotencyRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key && x.Route == route, ctx.RequestAborted);

        if (existing is not null)
        {
            ctx.Response.StatusCode = existing.StatusCode;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(existing.ResponseBodyJson, Encoding.UTF8);
            return;
        }

        // kaapataan response body
        var originalBody = ctx.Response.Body;
        await using var mem = new MemoryStream();
        ctx.Response.Body = mem;

        await _next(ctx);

        mem.Position = 0;
        var bodyText = await new StreamReader(mem).ReadToEndAsync();

        // palautetaan response normaalisti
        mem.Position = 0;
        await mem.CopyToAsync(originalBody);
        ctx.Response.Body = originalBody;

        // tallennetaan vain “onnistuneet” (2xx) idempotency-responsit
        if (ctx.Response.StatusCode >= 200 && ctx.Response.StatusCode < 300)
        {
            // varmistetaan valid JSON (fallback: wrap)
            var json = TryNormalizeJson(bodyText);

            db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = key,
                Route = route,
                StatusCode = ctx.Response.StatusCode,
                ResponseBodyJson = json
            });

            await db.SaveChangesAsync(ctx.RequestAborted);
        }
    }

    private static string TryNormalizeJson(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return body;
        }
        catch
        {
            return JsonSerializer.Serialize(new { raw = body });
        }
    }
}
