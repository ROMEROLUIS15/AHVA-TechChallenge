## ADDED Requirements

### Requirement: Acceso protegido al perfil
El sistema SHALL exigir una sesión autenticada para acceder a la página de perfil; los usuarios no autenticados SHALL ser redirigidos al login.

#### Scenario: Acceso sin sesión
- **WHEN** un usuario no autenticado intenta abrir la página de perfil
- **THEN** el sistema lo redirige a la pantalla de login

#### Scenario: Acceso con sesión válida
- **WHEN** un usuario autenticado abre la página de perfil
- **THEN** el sistema muestra la página de perfil con sus datos

### Requirement: Presentación de datos del usuario
El sistema SHALL mostrar los datos del usuario autenticado en la página de perfil, replicando la estructura del diseño: encabezado con nombre completo, rol y estado "Activo", y los campos de información básica (nombres, apellidos, tipo y número de documento, fecha de nacimiento, nacionalidad, sexo, correos, teléfonos, tipo y fecha de contratación).

#### Scenario: Se muestran los datos del usuario semilla
- **WHEN** el usuario semilla (July Camila Mendoza Quispe) inicia sesión y abre el perfil
- **THEN** el sistema muestra su nombre, rol, estado "Activo" y los campos de información básica con los valores de su registro

### Requirement: Perfil de solo lectura en este alcance
El sistema SHALL presentar la página de perfil como **solo lectura** en este alcance. Los controles de edición del diseño (íconos de lápiz en correo/teléfono) SHALL renderizarse por fidelidad visual pero SHALL NOT persistir cambios.

#### Scenario: Los datos del perfil no son editables
- **WHEN** el usuario visualiza la página de perfil
- **THEN** el sistema muestra los datos en solo lectura y los íconos de edición no realizan persistencia

### Requirement: Estructura de navegación del perfil
El sistema SHALL replicar la estructura visual del perfil: sidebar de navegación, topbar con identificación del usuario y notificaciones, y las tabs "Información básica", "Responsabilidades" e "Historial de responsabilidades", con "Información básica" activa por defecto.

#### Scenario: Navegación entre tabs
- **WHEN** el usuario está en la página de perfil
- **THEN** el sistema muestra las tres tabs con "Información básica" activa por defecto
- **AND** permite alternar la tab activa mediante interacción del lado del cliente
