## ADDED Requirements

<!-- Capacidad STRETCH: implementar solo si el alcance núcleo está terminado y sobra tiempo. -->

### Requirement: Aviso de expiración por inactividad
El sistema SHALL detectar la inactividad del usuario autenticado y, al alcanzar el umbral de inactividad configurado, mostrar un modal de aviso con una cuenta regresiva y una acción "Extender sesión". Los tiempos (umbral de inactividad y duración de la cuenta regresiva) SHALL ser configurables vía appsettings; los valores del diseño son 20 minutos de inactividad y 49 segundos de cuenta regresiva.

#### Scenario: Se alcanza el umbral de inactividad
- **WHEN** el usuario autenticado permanece inactivo hasta el umbral configurado
- **THEN** el sistema muestra el modal "Su sesión está a punto de expirar" con la cuenta regresiva y el botón "Extender sesión"

### Requirement: Extender la sesión
El sistema SHALL permitir extender la sesión desde el modal de aviso, cerrando el modal y reiniciando el temporizador de inactividad sin recargar la página ni perder el estado.

#### Scenario: El usuario extiende la sesión
- **WHEN** el usuario pulsa "Extender sesión" antes de que termine la cuenta regresiva
- **THEN** el sistema cierra el modal, reinicia el temporizador de inactividad y mantiene al usuario en el perfil

### Requirement: Expiración de la sesión
El sistema SHALL cerrar la sesión cuando la cuenta regresiva del aviso llegue a cero sin acción del usuario, redirigiendo al login y mostrando un toast informativo de sesión expirada.

#### Scenario: La sesión expira por inactividad
- **WHEN** la cuenta regresiva del modal termina sin que el usuario extienda la sesión
- **THEN** el sistema invalida la cookie de sesión, redirige al login
- **AND** muestra el toast "Su sesión ha expirado debido a inactividad. Por favor, inicie sesión nuevamente."
