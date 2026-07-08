## Context

Prueba técnica de AHVA para el rol de Programador web. Se evalúa capacidad de replicar un diseño (Figma, portal de acceso CEPLAN/PCM), estructurar una solución con buenas prácticas y entregar algo funcional en local, explicado por video. El entorno actual **no tiene el .NET SDK instalado**; sí hay Docker, Node 24 y OpenSpec. Restricción dura del proyecto: la arquitectura no se sacrifica por velocidad, y cada decisión se justifica con trade-offs. Deadline: 2026-07-08 20:00 (hora Venezuela), por lo que las decisiones buscan **máximo valor arquitectónico con mínimo riesgo de calendario**.

## Goals / Non-Goals

**Goals:**
- Solución ASP.NET Core MVC (.NET 8) en capas estilo Clean Architecture, con DI, tipado fuerte, async, manejo explícito de errores y seguridad por defecto.
- Replicar fielmente las pantallas del Figma con Bootstrap 5 y una interacción de cliente en TypeScript.
- Implementar la lógica de negocio evaluable: validación de credenciales, contador de fallos (CVF) y bloqueo temporal, con flujo de mensajes/estados.
- Ejecutable en local de forma reproducible (Docker para SQL Server, migraciones EF Core, seed determinista).

**Non-Goals:**
- Registro de usuarios, recuperación real de contraseña, o gestión de roles/permisos (fuera del diseño y del alcance).
- Las tabs "Responsabilidades" e "Historial" con datos reales (se maquetan; el foco de datos es "Información básica").
- Despliegue en la nube (el email lo permite en local).
- El timeout de sesión es **stretch**: solo si el núcleo está terminado.

## Decisions

### D1. UI: ASP.NET Core MVC (vs Razor Pages, Blazor)
**Elegido: MVC.** Es el patrón más reconocible y esperado en el ecosistema .NET empresarial peruano; separa claramente Controller/ViewModel/View, lo que evidencia arquitectura ante el evaluador. Razor Pages sería algo más simple para un CRUD de pocas páginas, pero MVC comunica mejor la separación de responsabilidades y encaja con Clean Architecture. Blazor se descarta: overkill y mayor riesgo siendo el primer proyecto en el framework bajo deadline.

### D2. Estructura en capas (Clean Architecture, 4 proyectos)
**Elegido: 4 proyectos** con la regla de dependencias hacia adentro:
- `Ceplan.Domain` — entidades (`User`), enums (`DocumentType` DNI/CE), errores/reglas de dominio, e interfaces de repositorio. Sin dependencias externas.
- `Ceplan.Application` — casos de uso/servicios (p. ej. `AuthenticationService`), DTOs, resultados tipados, e interfaces de infraestructura (`IPasswordHasher`, `IClock`, `IUserRepository`). Depende solo de Domain.
- `Ceplan.Infrastructure` — EF Core `DbContext`, implementación de repositorios, hasher, reloj del sistema, migraciones y seed. Depende de Application/Domain.
- `Ceplan.Web` — MVC (controllers, ViewModels, vistas Razor, TypeScript), wiring de DI y `appsettings`. Depende de Application e Infrastructure solo para composición.

**Alternativa considerada:** un solo proyecto con carpetas. Más rápido, pero la separación por proyectos **impone** la regla de dependencias en tiempo de compilación (Domain no puede referenciar EF Core), lo cual es exactamente lo que la prueba valora. **Trade-off:** más ceremonia para una app pequeña; se mitiga manteniendo las capas delgadas.

### D3. Acceso a datos: EF Core Code-First (vs Dapper, ADO.NET)
**Elegido: EF Core.** Migraciones versionadas, modelo en C#, y seed programático; el estándar moderno. El repositorio se expone tras `IUserRepository` (Application define la interfaz, Infrastructure la implementa) para no filtrar EF Core hacia dentro. Dapper/ADO.NET darían más control de SQL pero añaden plomería sin beneficio para este alcance.

### D4. Autenticación: cookie auth propia (vs ASP.NET Core Identity)
**Elegido: cookie authentication (`AddAuthentication().AddCookie`) + tabla `Users` propia.** El email pide "una tabla simple de usuarios"; Identity crea ~7 tablas y oculta la lógica que precisamente se quiere ver. Hacemos sign-in con `ClaimsPrincipal` y cookie HttpOnly/SameSite. **Trade-off:** asumimos manualmente hashing y verificación; se mitiga usando el `PasswordHasher<T>` oficial (no criptografía casera).

### D5. Hashing: `PasswordHasher<User>` / PBKDF2 (vs BCrypt)
**Elegido: `Microsoft.AspNetCore.Identity.PasswordHasher<T>`** (PBKDF2, incluido en el framework, sin dependencias extra, con formato versionado). BCrypt es válido pero añade un paquete NuGet sin ventaja aquí. Se expone tras `IPasswordHasher` en Application para poder sustituirlo (OCP).

### D6. Lógica de bloqueo (CVF) — dónde vive y cómo se calcula el tiempo
**Elegido:** el estado de bloqueo se persiste como columnas en `Users` (`FailedAttempts`, `LockoutEndUtc`), y la **regla** (umbral 5, duración 15 min) vive en el dominio/servicio, con umbral y duración leídos de `appsettings` (`LockoutOptions`). El cálculo de "¿está bloqueada?" usa una abstracción `IClock` en vez de `DateTime.UtcNow` directo → **testable** y evita acoplarse al reloj real. **Alternativa:** tabla separada de intentos; se descarta por sobreingeniería para el alcance.

### D7. Manejo explícito de errores: resultados tipados (vs excepciones para flujo)
**Elegido:** el caso de uso de login devuelve un **resultado tipado** (`LoginResult`: `Success | InvalidCredentials | AccountLocked | ValidationError`) que el controller mapea a vista/mensaje. Las excepciones se reservan para fallos realmente excepcionales (infra). Esto hace el flujo de control explícito y legible, sin usar excepciones como control de flujo. Los mensajes al usuario son **genéricos** (no revelan si el usuario existe) por seguridad.

### D8. Cliente: TypeScript compilado con `tsc` (sin bundler)
**Elegido:** TS compilado a `wwwroot/js` con `tsc` (config `tsconfig.json`, sin webpack/vite). Cumple el requisito de interacción de cliente con tipado, y evita el riesgo de configurar un bundler. Interacciones: mostrar/ocultar contraseña, validación en vivo del documento, y (stretch) el temporizador de sesión.

### D9. SQL Server en Docker (vs LocalDB)
**Elegido: Docker** (`docker-compose.yml`, imagen `mssql/server`), porque LocalDB no está instalado y Docker sí; es reproducible y no ensucia Windows. La cadena de conexión vive en `appsettings.Development.json` / user-secrets. **Trade-off:** el evaluador necesita Docker para correrlo; se mitiga documentando el arranque y porque la entrega principal es un video.

### D10. Async por defecto
Todo el IO (EF Core, sign-in) es `async/await`, con `CancellationToken` propagado desde los controllers a los servicios/repos. Sin `.Result`/`.Wait()`.

### D12. Control de errores global y flujo de mensajes centralizado
**Elegido:** middleware de manejo de excepciones (`UseExceptionHandler`) + página de error genérica + `UseStatusCodePages` para 404, con logging vía `ILogger` y sin filtrar detalles al usuario (en Development se permite la página de diagnóstico). Para el "flujo de mensajes" se usa un **patrón flash con `TempData`** (helper/partial de Bootstrap alerts) para éxito/error/info tras redirecciones. **Por qué:** el email evalúa explícitamente "control de errores" y "flujo de mensajes"; un manejo solo inline es insuficiente. **Trade-off:** un poco más de plomería inicial; se centraliza en un partial `_FlashMessages` y un filtro/handler reutilizable.

### D13. Gestión de secretos y variables de entorno (nada sensible al repo)
**Regla dura:** ninguna credencial, connection string real, contraseña de SQL Server ni variable de entorno sensible se versiona en GitHub. **Estrategia:**
- **Desarrollo local:** connection string y secretos vía **.NET User Secrets** (`dotnet user-secrets`, fuera del árbol del repo) y/o variables de entorno; `appsettings.Development.json` está en `.gitignore`.
- **Docker:** la contraseña de SQL Server se pasa por un archivo **`.env`** (ignorado); se versiona un **`.env.example`** con placeholders.
- **Plantillas versionadas:** `appsettings.json` solo con valores no sensibles/placeholders (`"__SET_VIA_USER_SECRETS__"`); se documenta en el README cómo poblar los secretos.
- **Configuración por capas de ASP.NET Core:** los valores reales se resuelven en tiempo de ejecución desde user-secrets/entorno, que sobreescriben appsettings.
- **`.gitignore` primero:** creado antes del primer commit; incluye `.env*`, `appsettings.*.local.json`, `appsettings.Development.json`, `*.pfx/*.key/*.pem`, `secrets.json`.
- **Verificación:** antes de publicar el repo, revisar `git status`/historial para confirmar que ningún secreto quedó trackeado.
**Trade-off:** el evaluador debe poblar sus propios secretos para correrlo; se mitiga con `.env.example` + instrucciones claras en el README y porque la entrega principal es el video.

### D11. Verificación E2E con Playwright MCP (local)
**Elegido:** validar los escenarios de las specs conduciendo la app real en el navegador mediante el **Playwright MCP** contra la instancia local (`https://localhost:<port>`). Cada escenario clave se ejerce de extremo a extremo (login OK → perfil, credenciales inválidas → error inline, 5 fallos → pantalla de bloqueo, acceso a perfil sin sesión → redirección a login) y se captura pantalla para contrastar fidelidad contra las 9 capturas del Figma. **Requisito previo:** el Playwright MCP no está conectado aún; se añade con `claude mcp add playwright -- npx @playwright/mcp@latest`. **Trade-off:** depende de configurar el MCP; si no estuviera disponible, el fallback es verificación manual + unit tests del servicio de login (que es testable gracias a `IClock`/DI). **Nota de fuente de diseño:** el archivo Figma no es accesible programáticamente (SPA con auth); las capturas en `design/figma-captures/` son la fuente de verdad visual.

## Risks / Trade-offs

- **[Sobrecoste de 4 proyectos bajo deadline]** → Capas delgadas; el andamiaje inicial (una sola pasada de `dotnet new` por proyecto + referencias) se hace primero y una sola vez.
- **[.NET SDK no instalado]** → Primer paso de implementación: instalar .NET 8 SDK y verificar `dotnet --version` antes de nada.
- **[Fidelidad visual al Figma consume tiempo]** → Priorizar layout y estados por Bootstrap + CSS puntual; recursos gráficos faltantes se sustituyen por equivalentes (permitido).
- **[El bloqueo depende del tiempo]** → `IClock` inyectable permite demostrar/expira sin esperar 15 min reales si hiciera falta, y hace la lógica testeable.
- **[Timeout de sesión (stretch) añade complejidad cliente+servidor]** → Se deja al final; tiempos configurables cortos para el video.
- **[Docker requerido]** → Documentar `docker compose up`; alternativa de fallback: connection string a cualquier SQL Server disponible.

## Migration Plan

Es un proyecto nuevo (sin sistema previo que migrar). Orden de puesta en marcha:
1. Instalar .NET 8 SDK; crear solución y los 4 proyectos con sus referencias.
2. `docker compose up -d` para SQL Server; configurar connection string.
3. Modelo `User` + `DbContext` + primera migración (`dotnet ef migrations add InitialCreate`) + `database update`.
4. Seed determinista del usuario del diseño (contraseña hasheada).
5. Autenticación por cookie + caso de uso de login (resultado tipado) + vistas de activación/login.
6. Lógica de CVF/bloqueo + pantalla de bloqueo.
7. Perfil protegido + maquetado fiel.
8. TypeScript (mostrar/ocultar contraseña, validación) + build en el pipeline de arranque.
9. (Stretch) timeout de sesión.
Rollback: no aplica (entorno local, control de versiones git).

## Resolved Decisions

- **Contraseña del usuario semilla:** `Ceplan2025$` (se persiste hasheada con PBKDF2; se documenta en el README para el login del video).
- **Tabs "Responsabilidades" / "Historial de responsabilidades":** se renderizan por fidelidad visual pero con **estado vacío / placeholder**; el foco de datos es "Información básica" (única tab detallada en el Figma).
- **Perfil de solo lectura:** los íconos de lápiz (correo/teléfono) son visuales; no se implementa edición/persistencia en este alcance (el foco es validación de login).
- **Login unificado:** el diseño muestra dos variantes de login (con y sin toggle DNI/CE). Se estandariza en la versión **con toggle**; el toast de "sesión expirada" se muestra sobre esa misma pantalla.
- **Entrega:** repositorio público de GitHub (link en el formulario) + video de pantalla + **explicación breve escrita** (qué se hizo, cómo se abordó, herramientas). El `.gitignore` excluye `bin/obj/node_modules`.
