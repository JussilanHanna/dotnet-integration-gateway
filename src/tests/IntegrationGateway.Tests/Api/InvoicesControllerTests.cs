using System.Net;
using System.Net.Http.Json;
using IntegrationGateway.Api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationGateway.Tests.Api;

public sealed class InvoicesControllerTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public InvoicesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostInvoice_WithValidPayload_ReturnsOk()
    {
        // Arrange
        var payload = new
        {
            externalInvoiceId = "TEST-INV-1",
            customer = new
            {
                externalCustomerId = "C-1",
                name = "Test Customer Oy"
            },
            invoiceDate = "2026-02-03",
            lines = new[]
            {
                new { description = "Consulting", quantity = 1, unitPrice = 100 }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/invoices")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Idempotency-Key", "test-key-1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(body);
    }
}
