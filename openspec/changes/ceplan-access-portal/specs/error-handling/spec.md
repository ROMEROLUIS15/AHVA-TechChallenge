## ADDED Requirements

### Requirement: Manejo global de excepciones
El sistema SHALL capturar las excepciones no controladas mediante un middleware/handler global, registrar el error con `ILogger`, y presentar al usuario una página de error genérica sin filtrar detalles técnicos (stack traces, SQL, etc.). En entorno de desarrollo SHALL poder mostrarse la página de diagnóstico detallada.

#### Scenario: Excepción no controlada en producción
- **WHEN** ocurre una excepción no manejada durante una petición en entorno no-desarrollo
- **THEN** el sistema registra el error con `ILogger` y muestra una página de error genérica
- **AND** no expone stack trace ni detalles internos al usuario

#### Scenario: Ruta inexistente
- **WHEN** el usuario navega a una ruta que no existe
- **THEN** el sistema responde con una página 404 acorde al estilo de la aplicación

### Requirement: Flujo de mensajes centralizado
El sistema SHALL disponer de un mecanismo central de mensajes (patrón flash basado en `TempData`) para comunicar resultados de éxito, error e información al usuario tras acciones y redirecciones, renderizados con estilos consistentes (alert/toast de Bootstrap).

#### Scenario: Mensaje tras cierre de sesión
- **WHEN** el usuario cierra sesión y es redirigido al login
- **THEN** el sistema muestra un mensaje flash informativo confirmando el cierre de sesión

#### Scenario: Mensaje de error de credenciales
- **WHEN** la validación de credenciales falla
- **THEN** el sistema muestra el mensaje de error mediante el mecanismo central, con estilo de error consistente y texto genérico
