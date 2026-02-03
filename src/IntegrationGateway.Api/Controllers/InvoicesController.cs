using FluentValidation;
using IntegrationGateway.Application.Abstractions;
using IntegrationGateway.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationGateway.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] InvoiceInDto dto,
        [FromServices] IValidator<InvoiceInDto> validator,
        [FromServices] IInvoiceService service,
        CancellationToken ct)
    {
        var v = await validator.ValidateAsync(dto, ct);
        if (!v.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                v.ToDictionary(x => x.PropertyName, x => x.Errors.Select(e => e.ErrorMessage).ToArray())
            ));
        }

        var result = await service.CreateInvoiceAsync(dto, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IInvoiceService service, CancellationToken ct)
    {
        var result = await service.GetInvoiceAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
