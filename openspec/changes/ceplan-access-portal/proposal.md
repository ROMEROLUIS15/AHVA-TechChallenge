## Why

La prueba técnica de AHVA exige una aplicación web funcional que replique fielmente el diseño de Figma de un portal de acceso gubernamental (CEPLAN / PCM) y que demuestre criterio de ingeniería: validación de usuario, control de errores y flujo de mensajes. El objetivo no es solo "que funcione el login", sino evidenciar arquitectura limpia, seguridad por defecto y decisiones justificadas, que es lo que el evaluador mide.

## What Changes

- Se crea una solución nueva ASP.NET Core MVC (.NET 8) estructurada en capas estilo Clean Architecture (Domain / Application / Infrastructure / Web).
- **Pantalla de activación de cuenta** ("¡Bienvenida, July!") como entrada al flujo, con botón a login.
- **Login** con toggle de tipo de documento DNI / CE, campo Usuario, campo Contraseña con mostrar/ocultar (interacción TypeScript), link "¿Olvidó su contraseña?" (placeholder) y link de soporte.
- **Validación de credenciales** contra una tabla `Users` en SQL Server, con contraseñas hasheadas (PBKDF2). Al validar correctamente se emite una cookie de autenticación.
- **Bloqueo por intentos fallidos (CVF)**: cada credencial inválida incrementa un contador; al 5.º fallo la cuenta se bloquea 15 minutos y se muestra la pantalla "Cuenta bloqueada temporalmente"; al vencer el bloqueo, el contador se resetea.
- **Control de errores y flujo de mensajes**: estados de error inline (bordes/textos rojos) y toasts (p. ej. sesión expirada), con mensajes genéricos que no filtran si el usuario existe.
- **Página de perfil de usuario** tras login exitoso, con tabs (Información básica / Responsabilidades / Historial), sidebar y topbar.
- **Datos semilla**: 1 usuario del diseño (July Camila Mendoza Quispe, DNI 07079879) con contraseña conocida y hasheada.
- **Infraestructura local**: SQL Server en Docker (docker-compose) + migraciones EF Core Code-First + build de TypeScript vía `tsc`.
- **Stretch (solo si sobra tiempo)**: timeout de sesión por inactividad con modal "Extender sesión", con tiempos configurables cortos para la grabación.

## Capabilities

### New Capabilities
- `user-authentication`: Pantalla de activación, login con toggle DNI/CE, validación de credenciales contra la tabla `Users` con hashing, emisión/cierre de la cookie de sesión, y mensajes de error/estado (incluye la interacción de mostrar/ocultar contraseña).
- `account-lockout`: Contador de validaciones fallidas (CVF), bloqueo temporal de 15 minutos al 5.º intento, pantalla "Cuenta bloqueada temporalmente", y reseteo del contador tras bloqueo o login exitoso.
- `user-profile`: Página de perfil de usuario protegida (requiere sesión) que muestra **en solo lectura** los datos del usuario autenticado con la estructura de tabs, sidebar y topbar del diseño.
- `error-handling`: Manejo global de excepciones (middleware + página de error genérica + logging) y un mecanismo central de mensajes flash (éxito/error/info) para el flujo de mensajes de la aplicación.
- `session-timeout` (stretch): Detección de inactividad, modal de aviso con cuenta regresiva y acción "Extender sesión"; al expirar, cierre de sesión y retorno al login con toast informativo. Tiempos configurables vía appsettings.

### Modified Capabilities
<!-- Ninguna: es un proyecto nuevo, no hay specs previas. -->

## Impact

- **Código nuevo**: solución .NET con proyectos `Domain`, `Application`, `Infrastructure`, `Web` (o carpetas equivalentes), vistas Razor + Bootstrap 5, y assets TypeScript compilados.
- **Base de datos**: nueva base con tabla `Users` (+ campos de bloqueo: contador de fallos, marca de bloqueo). Migraciones EF Core.
- **Dependencias**: EF Core (+ provider SQL Server), TypeScript (`tsc`), Bootstrap 5.
- **Infra local**: `docker-compose.yml` para SQL Server; requiere instalar el .NET 8 SDK (no presente aún en el entorno).
- **Configuración**: `appsettings.json` (cadena de conexión, umbral de CVF, duración de bloqueo, tiempos de sesión).
- **Entrega**: ejecución local + grabación de pantalla explicando enfoque, lógica y decisiones técnicas.
