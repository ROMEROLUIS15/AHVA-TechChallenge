# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

ASP.NET Core MVC (.NET 8) portal that replicates a Figma design for a CEPLAN/PCM
government access portal. Built spec-driven (OpenSpec) with Clean Architecture,
strong typing, and security-by-default. The evaluated business logic is: credential
validation, failed-attempt counter (CVF), and temporary account lockout.

Most docs, comments, and commit messages are in Spanish — match that language when
editing them.

## Commands

```powershell
# Run everything (Windows): puts .NET SDK on PATH, starts SQL Server (Docker), runs the app
.\run.ps1
# App serves at https://localhost:5443/ — starts on the Activation screen

# Manual run
dotnet run --project src/Ceplan.Web --urls "https://localhost:5443;http://localhost:5080"

dotnet build                              # build whole solution
docker compose up -d                      # SQL Server 2022 container only

# TypeScript client (optional — compiled app.js is committed)
cd src/Ceplan.Web
npm install
npm run build:ts      # tsc -> wwwroot/js/app.js  (also runs via MSBuild target if node_modules exists)
npm run watch:ts

# EF Core migrations (design-time factory: AppDbContextFactory.cs)
dotnet ef migrations add <Name> --project src/Ceplan.Infrastructure --startup-project src/Ceplan.Web
```

```powershell
dotnet test src/Ceplan.Tests/Ceplan.Tests.csproj    # xUnit unit tests
```

Tests cover `AuthenticationService` (credential validation, CVF counter, lockout, expiry)
using in-memory fakes and a controllable `FakeClock` — no DB or SQL Server needed.

### First-time setup (secrets are not committed)

`appsettings.json` holds only placeholders; real values come from .NET User Secrets or env vars:

```powershell
cp .env.example .env      # then set MSSQL_SA_PASSWORD (avoid the '$' char)
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost,1433;Database=CeplanAccessPortal;User Id=sa;Password=<pwd>;TrustServerCertificate=True" --project src/Ceplan.Web
dotnet user-secrets set "Seed:Password" "Ceplan2025$" --project src/Ceplan.Web
```

The Docker `MSSQL_SA_PASSWORD` and the connection-string password **must match**.
On startup the app auto-applies pending migrations and seeds the demo user (`Program.cs`).

Seed login: DNI `07079879` / password `Ceplan2025$`.

## Architecture

Clean Architecture in **4 projects**, dependencies pointing inward. The layering is
enforced at compile time (project references), which is the point — `Domain` cannot
reference EF Core or anything external.

```
Web  →  Application + Infrastructure        (composition only)
Infrastructure  →  Application  →  Domain
Domain  →  (nothing external)
```

- **Ceplan.Domain** — `User` entity, `DocumentType` enum (DNI/CE). Business rules live
  *inside* the entity: the lockout state (`FailedAttempts`, `LockoutEndUtc`) is mutated
  only through methods (`RegisterFailedAttempt`, `ClearExpiredLockout`,
  `RegisterSuccessfulLogin`, `IsLockedOut`). All setters are private; EF uses a private
  ctor and `User.Create(...)` is the only public factory. The domain never knows the
  hashing algorithm — passwords arrive already hashed.
- **Ceplan.Application** — use cases + **ports** (interfaces): `IUserRepository`,
  `IPasswordHasher`, `IClock`, `IAuthenticationService`. `AuthenticationService`
  orchestrates login and lockout policy against these ports only. Uses a **typed result**
  (`LoginResult` / `LoginStatus`) instead of exceptions for control flow — the controller
  maps status → view/message. `IClock` abstraction exists so lockout timing is testable.
- **Ceplan.Infrastructure** — EF Core `AppDbContext` + `EfUserRepository`, `SystemClock`,
  `PasswordHasherAdapter` (wraps `PasswordHasher<User>` / PBKDF2), migrations, `DbSeeder`,
  and `AddInfrastructure(connectionString)` DI wiring.
- **Ceplan.Web** — MVC controllers, Razor views, ViewModels, TypeScript. Cookie auth
  wiring, `appsettings`, and startup live here.

### Key flows

- **Login / lockout** (`AuthenticationService.LoginAsync`): unknown-or-inactive user and
  wrong password both return **generic** messages (never reveal whether the user exists).
  Expired lockouts are cleared before evaluating. On the Nth failed attempt
  (`Lockout:MaxFailedAttempts`, default 5) the account locks for `LockoutMinutes`
  (default 15). These are configurable in `appsettings.json` → `Lockout`.
- **Auth** is cookie-based (`AddAuthentication().AddCookie`) — **no ASP.NET Core Identity**,
  a deliberate choice (see `openspec/.../design.md` D4). Sign-in builds a `ClaimsPrincipal`
  manually in `AccountController.SignInAsync`. Cookie is HttpOnly / SameSite=Lax /
  Secure=Always, 60-min sliding expiration; forms use antiforgery tokens.
- **Session inactivity** (`Session` config, client-driven): a short client-side inactivity
  timer (TS) warns then hits `Account/SessionExpired`; `Account/KeepAlive` refreshes the
  sliding cookie. Distinct from the 60-min cookie lifetime.
- **Errors**: global `UseExceptionHandler("/Home/Error")` plus
  `UseStatusCodePagesWithReExecute` reuse one error page for 404s etc.
- **Default route** starts at `Account/Activation`, not Login, matching the design flow.

## Working in this repo

- **Spec-driven (OpenSpec).** Design and trade-offs are documented in
  `openspec/changes/ceplan-access-portal/` (`proposal.md`, `design.md`, `specs/`,
  `tasks.md`). Consult `design.md` before changing architectural decisions — each choice
  (MVC, 4 projects, EF Core, cookie auth, PBKDF2) is justified there with alternatives.
- **Respect the dependency rule.** Don't add external-infra references to Domain or
  Application; put concrete implementations in Infrastructure behind an Application port.
- **Keep domain invariants in the entity.** Don't mutate lockout/counter state from
  services or controllers — go through the `User` methods.
- **Secrets never get committed.** Keep real connection strings / passwords in User Secrets
  or env vars; `appsettings.json` stays placeholders only.
