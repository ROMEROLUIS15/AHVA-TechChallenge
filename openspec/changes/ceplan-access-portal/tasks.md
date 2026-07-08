## 1. Entorno y andamiaje de la solución

- [x] 1.1 Instalar el .NET 8 SDK y verificar `dotnet --version`
- [x] 1.2 Crear la solución `CeplanAccessPortal.sln` y los 4 proyectos: `Ceplan.Domain` (classlib), `Ceplan.Application` (classlib), `Ceplan.Infrastructure` (classlib), `Ceplan.Web` (mvc)
- [x] 1.3 Configurar las referencias entre proyectos según la regla de dependencias (Web→App/Infra, Infra→App/Domain, App→Domain)
- [x] 1.4 Añadir `.gitignore` (secretos + .NET) **antes** del primer commit e inicializar repositorio git
- [x] 1.4b Configurar gestión de secretos: `dotnet user-secrets init`, `appsettings.json` con placeholders y `.env.example` para Docker (sin valores reales)
- [x] 1.5 Añadir paquetes NuGet base (EF Core + SqlServer, EF Core Design en Web/Infra)

## 2. Infraestructura de datos (SQL Server + EF Core)

- [x] 2.1 Crear `docker-compose.yml` con SQL Server y documentar el arranque (`docker compose up -d`)
- [x] 2.2 Definir la entidad `User` en Domain (datos de perfil + `FailedAttempts`, `LockoutEndUtc`, `IsActive`) y el enum `DocumentType` (DNI/CE)
- [x] 2.3 Definir `IUserRepository` e `IClock` en Application; `IPasswordHasher` en Application
- [x] 2.4 Implementar `AppDbContext` + configuración de la entidad `User` en Infrastructure
- [x] 2.5 Implementar `EfUserRepository`, `SystemClock` y `PasswordHasherAdapter` (PBKDF2) en Infrastructure
- [x] 2.6 Configurar la connection string (user-secrets) y el registro de DI
- [x] 2.7 Crear la migración `InitialCreate` y aplicar `database update`
- [x] 2.8 Implementar seed determinista del usuario del diseño (July Camila Mendoza Quispe, DNI 07079879) con contraseña hasheada

## 3. Autenticación y flujo de login (spec: user-authentication)

- [x] 3.1 Configurar cookie authentication (HttpOnly/SameSite/Secure) y `[Authorize]` donde aplique
- [x] 3.2 Implementar `LoginResult` tipado (Success | InvalidCredentials | AccountLocked | ValidationError) en Application
- [x] 3.3 Implementar `AuthenticationService.LoginAsync` (async, valida hash, mapea a `LoginResult`) con mensajes genéricos
- [x] 3.4 Crear `AccountController` (GET activación, GET/POST login, POST logout) + ViewModels con DataAnnotations y antiforgery
- [x] 3.5 Maquetar vista de activación de cuenta fiel al diseño
- [x] 3.6 Maquetar vista de login (toggle DNI/CE, campos, links, header/footer) con estados de error inline
- [x] 3.7 Emitir `ClaimsPrincipal` y redirigir al perfil en login exitoso; implementar logout

## 4. Bloqueo por intentos fallidos (spec: account-lockout)

- [x] 4.1 Añadir `LockoutOptions` (umbral=5, duración=15min) enlazado desde appsettings
- [x] 4.2 Implementar la regla de dominio: incrementar CVF en fallo, bloquear al 5.º, resetear en éxito y al expirar (usando `IClock`)
- [x] 4.3 Rechazar login en cuenta bloqueada sin validar credenciales
- [x] 4.4 Maquetar la pantalla "Cuenta bloqueada temporalmente" con el mensaje del diseño

## 5. Perfil de usuario (spec: user-profile)

- [x] 5.1 Crear `ProfileController` protegido (`[Authorize]`) que carga el usuario autenticado (async)
- [x] 5.2 Crear `ProfileViewModel` mapeando los datos del usuario
- [x] 5.3 Maquetar la vista de perfil (sidebar, topbar, tabs, campos de "Información básica") fiel al diseño
- [x] 5.4 Redirección al login para usuarios no autenticados (verificar)

## 6. Interacciones de cliente en TypeScript

- [x] 6.1 Configurar `tsconfig.json` y el pipeline `tsc` → `wwwroot/js` (+ target MSBuild)
- [x] 6.2 Implementar mostrar/ocultar contraseña (ícono de ojo)
- [x] 6.3 Implementar validación en vivo del documento (DNI: 8 dígitos) y habilitación del botón "Ingresar"
- [x] 6.4 Implementar el cambio de tabs del perfil y mensajes (flash con Bootstrap)

## 7. Control de errores global y flujo de mensajes (spec: error-handling)

- [x] 7.1 Configurar `UseExceptionHandler` + `UseStatusCodePagesWithReExecute` (404) con logging `ILogger`
- [x] 7.2 Habilitar la página de diagnóstico solo en Development
- [x] 7.3 Implementar patrón flash con `TempData` (partial `_FlashMessages` con alerts de Bootstrap)
- [x] 7.4 Emitir mensajes flash en logout y sesión expirada; errores de credenciales/bloqueo mostrados inline/pantalla

## 8. Estilos y fidelidad visual (Bootstrap 5)

- [x] 8.1 Integrar Bootstrap 5 y definir el tema (rojo CEPLAN, azul primario, tipografía libre cercana)
- [x] 8.2 Layout compartido (`_Layout` público y `_LayoutApp`) con header, footer institucional y fondo andino
- [x] 8.3 Ajustar estados visuales (error, deshabilitado, info/warning) al diseño

## 9. (Stretch) Timeout de sesión (spec: session-timeout)

- [x] 9.1 Añadir `SessionTimeoutOptions` con tiempos configurables (cortos para el video; reales documentados)
- [x] 9.2 Implementar detección de inactividad + modal con cuenta regresiva y "Extender sesión" en TypeScript
- [x] 9.3 Endpoints `KeepAlive` (extender) y `SessionExpired` → logout + toast en login

## 10. Verificación

> Nota: el Playwright MCP no estaba conectado en el entorno; la verificación E2E se
> realizó de forma equivalente conduciendo la app real con `curl` (cookies + antiforgery).

- [ ] 10.1 Conectar el Playwright MCP (pendiente; se usó `curl` como alternativa)
- [x] 10.2 Unit tests del servicio de login/bloqueo con `IClock` fake (`Ceplan.Tests`, 10 casos)
- [x] 10.3 Verificado: login exitoso → redirige a perfil con datos del usuario semilla
- [x] 10.4 Verificado: credenciales inválidas → error inline ("Usuario o contraseña incorrectos.")
- [x] 10.5 Verificado: 5 intentos fallidos → pantalla "Cuenta bloqueada temporalmente"
- [x] 10.6 Verificado: acceso a perfil sin sesión → redirección al login; endpoints de sesión (KeepAlive/Expired)
- [ ] 10.7 Capturas de la app contrastadas contra `design/figma-captures/` (durante la grabación)

## 11. Entrega

- [x] 11.1 Escribir `README.md` (requisitos, arranque con Docker, credenciales del usuario semilla, decisiones)
- [x] 11.2 Redactar la "explicación breve del desarrollo" (`ENTREGA.md`) para el formulario
- [x] 11.3 Auditar que ningún secreto/connection string/`.env` quedó trackeado antes de publicar
- [x] 11.4 Publicar el código en un repositorio público de GitHub (7 commits temáticos, sin secretos)
- [ ] 11.5 Grabar el video explicando enfoque, lógica y decisiones técnicas
- [ ] 11.6 Completar el formulario de entrega (link del repo + video + explicación) antes del 2026-07-08 20:00 (hora Venezuela)
