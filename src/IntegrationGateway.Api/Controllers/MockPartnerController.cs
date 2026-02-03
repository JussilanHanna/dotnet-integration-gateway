using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers;

[ApiController]
[Route("partner/invoices")]
public sealed class MockPartnerController : ControllerBase
{
    private static int _counter = 0;

    [HttpPost]
    public IActionResult Receive([FromBody] object payload)
    {
        _counter++;

        // Simuloidaan ajoittaista epäonnistumista (joka 3. pyyntö)
        if (_counter % 3 == 0)
        {
            return StatusCode(503, new { message = "Temporary partner outage" });
        }

        return Ok(new { message = "Received", receivedAtUtc = DateTime.UtcNow });
    }
}
