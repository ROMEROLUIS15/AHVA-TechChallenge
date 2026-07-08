## 1. Entorno y andamiaje de la solución

- [x] 1.1 Instalar el .NET 8 SDK y verificar `dotnet --version`
- [ ] 1.2 Crear la solución `CeplanAccessPortal.sln` y los 4 proyectos: `Ceplan.Domain` (classlib), `Ceplan.Application` (classlib), `Ceplan.Infrastructure` (classlib), `Ceplan.Web` (mvc)
- [ ] 1.3 Configurar las referencias entre proyectos según la regla de dependencias (Web→App/Infra, Infra→App/Domain, App→Domain)
- [x] 1.4 Añadir `.gitignore` (secretos + .NET) **antes** del primer commit e inicializar repositorio git
- [ ] 1.4b Configurar gestión de secretos: `dotnet user-secrets init`, `appsettings.json` con placeholders y `.env.example` para Docker (sin valores reales)
- [ ] 1.5 Añadir paquetes NuGet base (EF Core + SqlServer, EF Core Design en Web/Infra)

## 2. Infraestructura de datos (SQL Server + EF Core)

- [x] 2.1 Crear `docker-compose.yml` con SQL Server y documentar el arranque (`docker compose up -d`)
- [ ] 2.2 Definir la entidad `User` en Domain (datos de perfil + `FailedAttempts`, `LockoutEndUtc`, `IsActive`) y el enum `DocumentType` (DNI/CE)
- [ ] 2.3 Definir `IUserRepository` e `IClock` en Application; `IPasswordHasher` en Application
- [ ] 2.4 Implementar `AppDbContext` + configuración de la entidad `User` en Infrastructure
- [ ] 2.5 Implementar `EfUserRepository`, `SystemClock` y `PasswordHasherAdapter` (PBKDF2) en Infrastructure
- [ ] 2.6 Configurar la connection string (appsettings.Development / user-secrets) y el registro de DI
- [ ] 2.7 Crear la migración `InitialCreate` y aplicar `database update`
- [ ] 2.8 Implementar seed determinista del usuario del diseño (July Camila Mendoza Quispe, DNI 07079879) con contraseña hasheada

## 3. Autenticación y flujo de login (spec: user-authentication)

- [ ] 3.1 Configurar cookie authentication (HttpOnly/SameSite) y `[Authorize]` por defecto donde aplique
- [ ] 3.2 Implementar `LoginResult` tipado (Success | InvalidCredentials | AccountLocked | ValidationError) en Application
- [ ] 3.3 Implementar `AuthenticationService.LoginAsync` (async, valida hash, mapea a `LoginResult`) con mensajes genéricos
- [ ] 3.4 Crear `AccountController` (GET activación, GET/POST login, POST logout) + ViewModels con DataAnnotations y antiforgery
- [ ] 3.5 Maquetar vista de activación de cuenta fiel al diseño
- [ ] 3.6 Maquetar vista de login (toggle DNI/CE, campos, links, header/footer) con estados de error inline
- [ ] 3.7 Emitir `ClaimsPrincipal` y redirigir al perfil en login exitoso; implementar logout

## 4. Bloqueo por intentos fallidos (spec: account-lockout)

- [ ] 4.1 Añadir `LockoutOptions` (umbral=5, duración=15min) enlazado desde appsettings
- [ ] 4.2 Implementar la regla de dominio: incrementar CVF en fallo, bloquear al 5.º, resetear en éxito y al expirar (usando `IClock`)
- [ ] 4.3 Rechazar login en cuenta bloqueada sin validar credenciales
- [ ] 4.4 Maquetar la pantalla "Cuenta bloqueada temporalmente" con el mensaje del diseño

## 5. Perfil de usuario (spec: user-profile)

- [ ] 5.1 Crear `ProfileController` protegido (`[Authorize]`) que carga el usuario autenticado (async)
- [ ] 5.2 Crear `ProfileViewModel` mapeando los datos del usuario
- [ ] 5.3 Maquetar la vista de perfil (sidebar, topbar, tabs, campos de "Información básica") fiel al diseño
- [ ] 5.4 Redirección al login para usuarios no autenticados (verificar)

## 6. Interacciones de cliente en TypeScript

- [ ] 6.1 Configurar `tsconfig.json` y el pipeline `tsc` → `wwwroot/js`
- [ ] 6.2 Implementar mostrar/ocultar contraseña (ícono de ojo)
- [ ] 6.3 Implementar validación en vivo del documento (DNI: 8 dígitos) y habilitación del botón "Ingresar"
- [ ] 6.4 Implementar el cambio de tabs del perfil y toasts de mensajes

## 7. Control de errores global y flujo de mensajes (spec: error-handling)

- [ ] 7.1 Configurar `UseExceptionHandler` + página de error genérica y `UseStatusCodePages` (404) con logging `ILogger`
- [ ] 7.2 Habilitar la página de diagnóstico solo en Development
- [ ] 7.3 Implementar patrón flash con `TempData` (partial `_FlashMessages` con alerts de Bootstrap)
- [ ] 7.4 Emitir mensajes flash en logout, sesión expirada y errores de credenciales/bloqueo

## 8. Estilos y fidelidad visual (Bootstrap 5)

- [ ] 8.1 Integrar Bootstrap 5 y definir el tema (rojo CEPLAN, azul primario, tipografía libre cercana)
- [ ] 8.2 Layout compartido (`_Layout`) con header, footer institucional y fondo andino
- [ ] 8.3 Ajustar estados visuales (error, deshabilitado, info/warning) al diseño

## 9. (Stretch) Timeout de sesión (spec: session-timeout)

- [ ] 9.1 Añadir `SessionOptions` con tiempos configurables (cortos para el video; reales documentados)
- [ ] 9.2 Implementar detección de inactividad + modal con cuenta regresiva y "Extender sesión" en TypeScript
- [ ] 9.3 Endpoint para extender sesión y flujo de expiración → logout + toast en login

## 10. Verificación (Playwright MCP)

- [ ] 10.1 Conectar el Playwright MCP (`claude mcp add playwright -- npx @playwright/mcp@latest`) y reiniciar Claude Code
- [ ] 10.2 (Opcional pero recomendado) Unit tests del servicio de login/bloqueo usando `IClock` fake (5.º intento bloquea, expiración resetea)
- [ ] 10.3 E2E con Playwright: login exitoso → redirige a perfil con datos del usuario semilla
- [ ] 10.4 E2E con Playwright: credenciales inválidas → estado de error inline (mensaje genérico)
- [ ] 10.5 E2E con Playwright: 5 intentos fallidos → pantalla "Cuenta bloqueada temporalmente"
- [ ] 10.6 E2E con Playwright: acceso a perfil sin sesión → redirección al login; mostrar/ocultar contraseña funciona
- [ ] 10.7 Capturar pantallas de la app en cada pantalla y contrastar fidelidad contra `design/figma-captures/`

## 11. Entrega

- [ ] 11.1 Escribir `README.md` (requisitos, arranque con Docker, credenciales del usuario semilla, decisiones)
- [ ] 11.2 Redactar la "explicación breve del desarrollo" (qué se hizo, cómo se abordó, herramientas) para el formulario
- [ ] 11.3 Auditar que ningún secreto/connection string/`.env` quedó trackeado (revisar `git status` e historial) antes de publicar
- [ ] 11.4 Publicar el código en un repositorio público de GitHub (sin `bin/obj/node_modules` ni secretos)
- [ ] 11.5 Grabar el video explicando enfoque, lógica y decisiones técnicas
- [ ] 11.6 Completar el formulario de entrega (link del repo + video + explicación) antes del 2026-07-08 20:00 (hora Venezuela)
