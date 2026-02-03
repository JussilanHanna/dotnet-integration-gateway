using FluentValidation;
using IntegrationGateway.Api.Idempotency;
using IntegrationGateway.Application.Abstractions;
using IntegrationGateway.Application.Dtos;
using IntegrationGateway.Application.Validation;
using IntegrationGateway.Infrastructure.Data;
using IntegrationGateway.Infrastructure.Partner;
using IntegrationGateway.Infrastructure.Services;
using IntegrationGateway.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF Core (SQLite)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=integration.db"));

// Validation
builder.Services.AddScoped<IValidator<InvoiceInDto>, InvoiceInDtoValidator>();

// App services
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Partner HttpClient (osoitetaan omaan API:in demo-mielessä)
builder.Services.AddHttpClient<IPartnerClient, PartnerClient>(http =>
{
    http.BaseAddress = new Uri(builder.Configuration["Partner:BaseUrl"] ?? "http://localhost:5000");
});

// Worker
builder.Services.AddHostedService<InvoiceSenderWorker>();

var app = builder.Build();

// DB migrate (dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

// Idempotency (vain POST /api/invoices -reitin edessä)
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/api/invoices") && HttpMethods.IsPost(ctx.Request.Method),
    branch => branch.UseMiddleware<IdempotencyMiddleware>()
);

app.MapControllers();

app.Run();
