# Portal de acceso CEPLAN — Prueba Técnica AHVA

Aplicación web ASP.NET Core que replica el diseño de Figma de un portal de acceso
gubernamental (CEPLAN / PCM) e implementa validación de usuario, control de errores
y flujo de mensajes. Desarrollada bajo **Spec-Driven Development** (OpenSpec) y con
foco en arquitectura limpia, tipado fuerte y seguridad por defecto.

## Stack

| Área | Tecnología |
|---|---|
| Framework | ASP.NET Core **MVC** (.NET 8) |
| Front-end | **Bootstrap 5** + CSS propio (tema CEPLAN) |
| Cliente | **TypeScript** (compilado con `tsc` → `wwwroot/js/app.js`) |
| Datos | **EF Core 8** Code-First + migraciones |
| Base de datos | **SQL Server** (contenedor Docker) |
| Autenticación | **Cookie auth** propia (sin ASP.NET Core Identity), hashing **PBKDF2** |

## Arquitectura (Clean Architecture)

Cuatro proyectos de producción con la regla de dependencias hacia adentro, más un
proyecto de pruebas:

```
src/
├── Ceplan.Domain/                 # Entidades y reglas de negocio (sin dependencias externas)
│   ├── Entities/User.cs           #   entidad con las reglas de bloqueo encapsuladas
│   └── Enums/DocumentType.cs      #   DNI / CE
│
├── Ceplan.Application/            # Casos de uso + puertos (depende solo de Domain)
│   ├── Abstractions/              #   IUserRepository, IPasswordHasher, IClock
│   ├── Authentication/            #   AuthenticationService, LoginResult (resultado tipado)
│   └── Options/                   #   LockoutOptions, SessionTimeoutOptions
│
├── Ceplan.Infrastructure/        # Implementaciones (depende de Application/Domain)
│   ├── Persistence/               #   AppDbContext, EfUserRepository, DbSeeder, Migrations
│   ├── Security/                  #   PasswordHasherAdapter (PBKDF2)
│   ├── Time/SystemClock.cs
│   └── DependencyInjection.cs     #   wiring de DI
│
├── Ceplan.Web/                   # MVC — composición (depende de Application e Infrastructure)
│   ├── Controllers/               #   Account, Profile, Home
│   ├── Models/                    #   ViewModels
│   ├── Views/                     #   Razor (Account, Profile, Home, Shared)
│   ├── Scripts/app.ts             #   TypeScript → wwwroot/js/app.js
│   ├── wwwroot/                   #   css/site.css, img/ (escudo-peru.svg, ceplan-logo.png), js/
│   └── Program.cs                 #   pipeline, cookie auth, migración + seed al arrancar
│
└── Ceplan.Tests/                 # xUnit — pruebas de AuthenticationService (login/CVF/bloqueo)
    ├── AuthenticationServiceTests.cs
    └── Fakes/                     #   FakeClock, FakePasswordHasher, FakeUserRepository, TestUser
```

Dependencias hacia adentro: `Web → Application/Infrastructure`, `Infrastructure → Application → Domain`.
`Domain` no referencia nada externo (regla impuesta en compilación).

## Funcionalidad

- **Activación de cuenta** → **Login** (toggle DNI/CE, mostrar/ocultar contraseña, validación en vivo).
- **Validación de credenciales** contra la tabla `Users` con contraseñas hasheadas.
- **Bloqueo por intentos fallidos (CVF):** al 5.º fallo la cuenta se bloquea 15 minutos
  ("Cuenta bloqueada temporalmente"); el bloqueo expira y el contador se resetea.
- **Perfil de usuario** protegido (requiere sesión) con tabs, sidebar y topbar.
- **Timeout de sesión por inactividad:** aviso con cuenta regresiva ("Extender sesión") y,
  al expirar, cierre de sesión con vuelta al login (toast). Detección en cliente (TypeScript).
- **Control de errores** (manejo global + página de error) y **flujo de mensajes** (flash + estados inline).
- **Fidelidad al diseño:** identificador institucional del Estado Peruano (Escudo Nacional)
  en el footer y logotipo de CEPLAN en el topbar, según los captures.

Parámetros configurables en `appsettings.json`: sección `Lockout` (umbral e intervalo del
bloqueo) y sección `Session` (inactividad y cuenta regresiva).

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (para SQL Server)
- (Opcional) Node.js, solo si quieres recompilar el TypeScript

## Puesta en marcha (local)

### Primera vez (configuración inicial)

```bash
# 1. Levantar SQL Server (crea .env desde la plantilla y elige una contraseña fuerte para 'sa')
cp .env.example .env
#   edita .env -> MSSQL_SA_PASSWORD=TuPasswordFuerte123!   (evita el carácter '$')
docker compose up -d

# 2. Configurar los secretos (NO se versionan; se leen en runtime)
dotnet user-secrets set "ConnectionStrings:Default" \
  "Server=localhost,1433;Database=CeplanAccessPortal;User Id=sa;Password=TuPasswordFuerte123!;TrustServerCertificate=True" \
  --project src/Ceplan.Web
dotnet user-secrets set "Seed:Password" "Ceplan2025$" --project src/Ceplan.Web

# 3. Ejecutar (aplica migraciones y siembra el usuario automáticamente al arrancar)
dotnet run --project src/Ceplan.Web
```

La app queda en `https://localhost:5443` (o el puerto que indique la consola).

> La contraseña de `MSSQL_SA_PASSWORD` (Docker) y la del connection string **deben coincidir**.

### Cada vez que quieras iniciar el proyecto (Windows / PowerShell)

El repositorio incluye un script `run.ps1` que deja el .NET SDK en el PATH de la
sesión, levanta SQL Server (Docker) y arranca la app en un solo paso:

```powershell
cd C:\dev\AHVA-TechChallenge
.\run.ps1
```

Luego abre 👉 **https://localhost:5443/** — la app inicia en la pantalla de
**Activación** y desde "Iniciar sesión" pasa al Login (según el flujo del diseño).
Para detener la app: `Ctrl + C`.

> **Nota (una sola vez):** si PowerShell bloquea la ejecución de scripts, habilítalos con
> `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` y vuelve a ejecutar `.\run.ps1`.
>
> Si prefieres usar `dotnet` directamente, primero asegúrate de que el SDK esté en el PATH
> (`$env:Path = "$HOME\.dotnet;$env:Path"`) o abre una terminal nueva tras instalarlo.

### Credenciales del usuario semilla (para el login)

| Campo | Valor |
|---|---|
| Tipo de documento | DNI |
| Usuario | `07079879` |
| Contraseña | `Ceplan2025$` |

## Pruebas

Pruebas unitarias del caso de uso de login y la política de bloqueo (xUnit + dobles en
memoria y un `IClock` falso, sin BD ni esperas reales):

```bash
dotnet test src/Ceplan.Tests
```

Cubren: credenciales válidas/inválidas, usuario inexistente/inactivo, contador de fallos (CVF),
bloqueo al 5.º intento, rechazo sin validar contraseña estando bloqueado y expiración del bloqueo.

## Recompilar el TypeScript (opcional)

```bash
cd src/Ceplan.Web
npm install
npm run build:ts   # genera wwwroot/js/app.js
```

## Seguridad / secretos

- Ningún secreto se versiona: `.env`, connection strings reales y contraseñas se resuelven
  desde **.NET User Secrets** o variables de entorno. `appsettings.json` solo contiene placeholders.
- Contraseñas hasheadas con **PBKDF2** (`PasswordHasher`); cookie **HttpOnly/SameSite/Secure**;
  formularios protegidos con **antiforgery**; mensajes de error genéricos (no revelan si el usuario existe).

## Especificaciones (OpenSpec)

El diseño y las decisiones (con trade-offs) están en `openspec/changes/ceplan-access-portal/`
(`proposal.md`, `design.md`, `specs/`, `tasks.md`).
