## ADDED Requirements

### Requirement: Pantalla de activación de cuenta
El sistema SHALL presentar una pantalla de activación de cuenta como entrada al flujo, replicando el diseño ("¡Bienvenida, {nombre}!", mensaje de cuenta activada) con un botón que dirige al login.

#### Scenario: Usuario llega a la activación y continúa al login
- **WHEN** el usuario visita la ruta de activación de cuenta
- **THEN** el sistema muestra el mensaje de bienvenida, el ícono y el botón "Iniciar sesión"
- **AND** al pulsar "Iniciar sesión" el sistema navega a la pantalla de login

### Requirement: Renderizado de la pantalla de login
El sistema SHALL mostrar una pantalla de login fiel al diseño, con un selector de tipo de documento DNI / CE, campo Usuario, campo Contraseña, enlace "¿Olvidó su contraseña?", enlace de ayuda de soporte, y el header/footer institucionales.

#### Scenario: Se muestra el formulario de login vacío
- **WHEN** el usuario abre la pantalla de login sin datos previos
- **THEN** el sistema muestra el toggle DNI/CE (DNI seleccionado por defecto), los campos Usuario y Contraseña vacíos con sus placeholders, y el botón "Ingresar"

#### Scenario: Cambio de tipo de documento
- **WHEN** el usuario alterna el selector entre DNI y CE
- **THEN** el sistema refleja el tipo de documento seleccionado para la validación del campo Usuario

### Requirement: Interacción de mostrar/ocultar contraseña
El sistema SHALL permitir alternar la visibilidad de la contraseña mediante un control (ícono de ojo) en el campo Contraseña, implementado como interacción del lado del cliente en TypeScript.

#### Scenario: Alternar visibilidad de la contraseña
- **WHEN** el usuario pulsa el ícono de ojo con texto ingresado en el campo Contraseña
- **THEN** el sistema alterna el input entre tipo `password` y `text` sin recargar la página
- **AND** el estado del ícono refleja si la contraseña está visible u oculta

### Requirement: Validación de credenciales
El sistema SHALL validar las credenciales enviadas contra la tabla `Users`, comparando la contraseña mediante verificación de hash, de forma asíncrona.

#### Scenario: Credenciales válidas
- **WHEN** el usuario envía un Usuario y Contraseña que coinciden con un registro activo y no bloqueado
- **THEN** el sistema autentica al usuario, emite la cookie de sesión y redirige a la página de perfil

#### Scenario: Credenciales inválidas
- **WHEN** el usuario envía credenciales que no coinciden con ningún registro válido
- **THEN** el sistema rechaza el acceso, muestra el login en estado de error con mensaje inline
- **AND** el mensaje de error es genérico y no revela si el Usuario existe o no

### Requirement: Validación de entrada del formulario
El sistema SHALL validar la entrada del formulario tanto en el cliente como en el servidor: Usuario y Contraseña requeridos, y formato del Usuario acorde al tipo de documento (DNI: 8 dígitos numéricos).

#### Scenario: Envío con campos vacíos
- **WHEN** el usuario intenta enviar el formulario con Usuario o Contraseña vacíos
- **THEN** el sistema impide el envío y señala los campos requeridos con estado de error

#### Scenario: Formato de DNI inválido
- **WHEN** el tipo de documento es DNI y el Usuario no tiene 8 dígitos numéricos
- **THEN** el sistema marca el campo como inválido y no procesa la autenticación

### Requirement: Seguridad por defecto de la autenticación
El sistema MUST almacenar las contraseñas hasheadas (PBKDF2 vía PasswordHasher), emitir la cookie de autenticación como HttpOnly y con SameSite, y proteger los formularios con antiforgery token. El sistema MUST NOT almacenar contraseñas en texto plano ni registrarlas en logs. El sistema MUST NOT versionar en el repositorio secretos ni connection strings reales; estos SHALL resolverse desde User Secrets o variables de entorno.

#### Scenario: Ningún secreto en el repositorio
- **WHEN** se prepara el repositorio para publicar
- **THEN** no existen connection strings reales, contraseñas ni variables de entorno sensibles trackeadas por git
- **AND** los valores sensibles se resuelven en runtime desde User Secrets o variables de entorno

#### Scenario: La contraseña nunca se almacena en claro
- **WHEN** se siembra o crea un usuario
- **THEN** la contraseña persistida es un hash y no el texto original

#### Scenario: Protección antiforgery del login
- **WHEN** se envía el formulario de login sin un antiforgery token válido
- **THEN** el sistema rechaza la solicitud

### Requirement: Cierre de sesión
El sistema SHALL permitir cerrar la sesión, eliminando la cookie de autenticación y devolviendo al usuario a la pantalla de login.

#### Scenario: El usuario cierra sesión
- **WHEN** un usuario autenticado ejecuta la acción de cerrar sesión
- **THEN** el sistema invalida la cookie y redirige al login
