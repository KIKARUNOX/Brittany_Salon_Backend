# OpenCode Agent Instructions: Brittany Salon Backend

This file provides high-signal context for AI agents working in this repository.

## Architecture & Project Structure
- **Framework:** .NET 10 Web API.
- **Clean Architecture Layout:** Despite documentation mentioning a `src/` folder, the layers (`Api`, `Application`, `Domain`, `Infrastructure`) are located directly inside `Brittany_Salon_Backend/`.
- **Primary Entry Point:** `Brittany_Salon_Backend/Api/Program.cs` (Configures DI, Swagger, JWT Auth, and DbContext).
- **Static Files:** Served from `Brittany_Salon_Backend/public/` and mapped to the root URL path (`/`).
- **CORS:** Configured to accept requests from `http://localhost:3000` (the frontend).

## Testing & Toolchain
- **Testing Framework:** `xUnit` with `Moq` and `Microsoft.EntityFrameworkCore.InMemory`.
- **Test Project:** Located at `Brittany_Salon_Backend.Tests/`.
- **Seeding Script:** A separate utility exists at `scripts/SeedUsers/SeedUsers.csproj`. Note that it uses **.NET 8** and `Microsoft.Data.SqlClient` instead of EF Core.

## Developer Commands
- **Run Backend:**
  `dotnet run --project Brittany_Salon_Backend/Brittany_Salon_Backend.csproj`
- **Run Tests:**
  `dotnet test Brittany_Salon_Backend.Tests/Brittany_Salon_Backend.Tests.csproj`
- **Run Seed Script:**
  `dotnet run --project scripts/SeedUsers/SeedUsers.csproj`
- **Add Packages:** Ensure you run `dotnet add package` within the specific project directory (`Brittany_Salon_Backend/` or `Brittany_Salon_Backend.Tests/`), not the root workspace.

## Quirks & Conventions
- **Logging:** Uses a conditional `IDevLogger` (`DevLogger` in Development, `NullDevLogger` otherwise).
- **Database:** PostgreSQL via Supabase (`appsettings.json`). For local dev use `dotnet user-secrets` (Resend API key required).
- **Solution file:** `Brittany_Salon_Back.sln` at `Brittany_Salon_Backend/` root (not inside project folder).
- **Migrations:** Run `dotnet ef` commands targeting the `Brittany_Salon_Backend` project where `AppDbContext` is defined.