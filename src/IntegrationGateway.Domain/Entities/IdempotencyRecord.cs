namespace IntegrationGateway.Domain.Entities;

public sealed class IdempotencyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Key { get; set; } = default!;
    public string Route { get; set; } = default!;

    public int StatusCode { get; set; }
    public string ResponseBodyJson { get; set; } = default!;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
