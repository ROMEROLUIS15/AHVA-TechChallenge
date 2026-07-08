# Explicación breve del desarrollo

**¿Qué hice?**
Desarrollé una aplicación web en **ASP.NET Core MVC (.NET 8)** que replica el portal de
acceso del diseño de Figma (CEPLAN/PCM): pantalla de activación, login con selector
DNI/CE, validación de credenciales, bloqueo temporal por intentos fallidos y página de
perfil de usuario. Incluye la lógica pedida: validación de usuario, control de errores
y flujo de mensajes.

**¿Cómo lo abordé?**
Trabajé **spec-driven** (OpenSpec): antes de codificar definí propuesta, diseño con
trade-offs, especificaciones por capacidad y tareas. Estructuré la solución en
**Clean Architecture** (Domain / Application / Infrastructure / Web) con inyección de
dependencias, tipado fuerte, programación asíncrona, manejo explícito de errores
(resultado tipado para el login) y seguridad por defecto (hashing PBKDF2, cookie
HttpOnly/SameSite, antiforgery, mensajes genéricos). La regla de bloqueo (5 intentos →
15 min) vive en el dominio y usa una abstracción de reloj para ser testeable. Los
secretos no se versionan (User Secrets / variables de entorno).

**¿Qué herramientas usé?**
ASP.NET Core MVC, EF Core 8 (Code-First + migraciones), SQL Server en Docker,
Bootstrap 5 + CSS propio para replicar el tema, TypeScript (compilado con `tsc`) para
las interacciones de cliente (mostrar/ocultar contraseña, validación en vivo, tabs),
y OpenSpec para el flujo spec-driven. Verifiqué el flujo de extremo a extremo (login
correcto → perfil, credenciales inválidas, bloqueo al 5.º intento, acceso protegido).

**Usuario de prueba:** DNI `07079879` · contraseña `Ceplan2025$`.
