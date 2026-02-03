# dotnet-integration-gateway

# .NET Integration Gateway

ASP.NET Core Web API demo, joka simuloi tuotantotasoista integraatiopalvelua:
laskujen vastaanotto, validointi, idempotenssi, tallennus ja edelleenlähetys
ulkoiseen järjestelmään.

Tämä projekti on tarkoitettu **portfolio- ja demo-käyttöön**, ja se kuvaa
tyypillisiä integraatiokehityksen haasteita ja ratkaisuja .NET-ympäristössä.

---

## ✨ Features

- REST API (ASP.NET Core)
- Invoice ingestion (`POST /api/invoices`)
- Input validation (FluentValidation)
- Idempotency support (`Idempotency-Key` header)
- Persistence with EF Core (SQLite)
- Background worker for outbound delivery & retries
- Simulated partner API (mock endpoint)
- Structured logging (Serilog)
- Swagger UI

---

## 🧱 Architecture

The solution follows a layered architecture:

- **Api**
  - HTTP endpoints, middleware, dependency injection
- **Application**
  - DTOs, validation, service interfaces
- **Domain**
  - Core business entities and enums
- **Infrastructure**
  - EF Core, HttpClient integrations, background workers

