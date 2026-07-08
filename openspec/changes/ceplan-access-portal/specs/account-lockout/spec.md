## ADDED Requirements

### Requirement: Contador de validaciones fallidas (CVF)
El sistema SHALL mantener por usuario un contador de validaciones fallidas que se incrementa en cada intento de login con credenciales inválidas.

#### Scenario: Un intento fallido incrementa el contador
- **WHEN** un usuario existente falla la validación de credenciales y su cuenta no está bloqueada
- **THEN** el sistema incrementa en 1 el contador de validaciones fallidas de esa cuenta
- **AND** muestra el login en estado de error inline

#### Scenario: Intentos por debajo del umbral permiten reintentar
- **WHEN** el contador de validaciones fallidas es menor que 5 tras un fallo
- **THEN** el sistema permite un nuevo intento de login

### Requirement: Bloqueo temporal por umbral de intentos
El sistema SHALL bloquear temporalmente la cuenta durante 15 minutos cuando el contador de validaciones fallidas alcance 5, y mostrar la pantalla "Cuenta bloqueada temporalmente". El umbral y la duración SHALL ser configurables vía appsettings.

#### Scenario: El quinto fallo bloquea la cuenta
- **WHEN** un intento fallido lleva el contador de validaciones fallidas a 5
- **THEN** el sistema marca la cuenta como bloqueada con una marca de tiempo de expiración a 15 minutos
- **AND** muestra la pantalla "Cuenta bloqueada temporalmente" con el mensaje del diseño

### Requirement: Rechazo de login en cuenta bloqueada
El sistema SHALL rechazar cualquier intento de login mientras la cuenta esté bloqueada, sin validar las credenciales, mostrando la pantalla de bloqueo.

#### Scenario: Login sobre cuenta bloqueada
- **WHEN** el usuario envía credenciales (válidas o no) mientras la cuenta está dentro del periodo de bloqueo
- **THEN** el sistema no valida las credenciales y muestra la pantalla "Cuenta bloqueada temporalmente"

### Requirement: Expiración del bloqueo y reseteo del contador
El sistema SHALL considerar la cuenta desbloqueada una vez transcurrido el periodo de bloqueo, reseteando el contador de validaciones fallidas para permitir nuevos intentos.

#### Scenario: El bloqueo expira
- **WHEN** el usuario intenta iniciar sesión después de vencido el periodo de bloqueo de 15 minutos
- **THEN** el sistema trata la cuenta como no bloqueada, resetea el contador y valida las credenciales normalmente

### Requirement: Reseteo del contador tras login exitoso
El sistema SHALL resetear el contador de validaciones fallidas a cero cuando un login sea exitoso.

#### Scenario: Login exitoso limpia el contador
- **WHEN** un usuario con intentos fallidos previos (por debajo del umbral) inicia sesión correctamente
- **THEN** el sistema resetea su contador de validaciones fallidas a cero
