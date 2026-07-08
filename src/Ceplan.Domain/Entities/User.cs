using Ceplan.Domain.Enums;

namespace Ceplan.Domain.Entities;

/// <summary>
/// Usuario del portal de acceso. Agrega los datos de perfil mostrados en el diseño
/// y el estado de seguridad para la validación de login (contador de intentos y bloqueo).
/// La entidad encapsula sus reglas: nadie fuera de ella muta el contador o el bloqueo.
/// </summary>
public class User
{
    // Constructor sin parámetros requerido por EF Core (privado: no forma parte de la API pública).
    private User()
    {
        DocumentNumber = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        FirstLastName = string.Empty;
        SecondLastName = string.Empty;
        Role = string.Empty;
        Nationality = string.Empty;
        Sex = string.Empty;
        PrimaryEmail = string.Empty;
        Organization = string.Empty;
        ContractType = string.Empty;
    }

    public int Id { get; private set; }

    // --- Credenciales ---
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; }
    public string PasswordHash { get; private set; }

    // --- Datos de perfil (diseño: "Información básica") ---
    public string FirstName { get; private set; }        // Nombres
    public string FirstLastName { get; private set; }    // Primer Apellido
    public string SecondLastName { get; private set; }   // Segundo Apellido
    public string Role { get; private set; }             // p. ej. "Administrador de Recursos"
    public string Organization { get; private set; }     // p. ej. "011 Ministerio de Salud"
    public DateOnly BirthDate { get; private set; }      // Fecha de nacimiento
    public string Nationality { get; private set; }      // Nacionalidad
    public string Sex { get; private set; }              // Sexo
    public string PrimaryEmail { get; private set; }     // Correo electrónico principal
    public string? SecondaryEmail { get; private set; }  // Correo electrónico secundario (opcional)
    public string? MobilePhone { get; private set; }     // Teléfono móvil
    public string? SecondaryPhone { get; private set; }  // Teléfono secundario (opcional)
    public string ContractType { get; private set; }     // Tipo de Contratación (p. ej. "CAS")
    public DateOnly ContractDate { get; private set; }   // Fecha de contratación
    public bool IsActive { get; private set; }           // Estado "Activo"

    // --- Estado de seguridad (bloqueo por intentos) ---
    public int FailedAttempts { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }

    /// <summary>
    /// Crea un usuario. La contraseña ya debe venir hasheada (el dominio no conoce el algoritmo).
    /// </summary>
    public static User Create(
        DocumentType documentType,
        string documentNumber,
        string passwordHash,
        string firstName,
        string firstLastName,
        string secondLastName,
        string role,
        string organization,
        DateOnly birthDate,
        string nationality,
        string sex,
        string primaryEmail,
        string contractType,
        DateOnly contractDate,
        bool isActive = true,
        string? secondaryEmail = null,
        string? mobilePhone = null,
        string? secondaryPhone = null)
    {
        return new User
        {
            DocumentType = documentType,
            DocumentNumber = documentNumber,
            PasswordHash = passwordHash,
            FirstName = firstName,
            FirstLastName = firstLastName,
            SecondLastName = secondLastName,
            Role = role,
            Organization = organization,
            BirthDate = birthDate,
            Nationality = nationality,
            Sex = sex,
            PrimaryEmail = primaryEmail,
            SecondaryEmail = secondaryEmail,
            MobilePhone = mobilePhone,
            SecondaryPhone = secondaryPhone,
            ContractType = contractType,
            ContractDate = contractDate,
            IsActive = isActive,
            FailedAttempts = 0,
            LockoutEndUtc = null
        };
    }

    /// <summary>Nombre completo en el formato del diseño: "Apellido Apellido, Nombres".</summary>
    public string FullName => $"{FirstLastName} {SecondLastName}, {FirstName}".Trim();

    /// <summary>Indica si la cuenta está bloqueada en el instante indicado.</summary>
    public bool IsLockedOut(DateTime utcNow) => LockoutEndUtc.HasValue && LockoutEndUtc.Value > utcNow;

    /// <summary>
    /// Si el bloqueo ya expiró, limpia el estado (contador y marca de bloqueo) para permitir reintentos.
    /// Debe invocarse antes de evaluar un nuevo intento de login.
    /// </summary>
    public void ClearExpiredLockout(DateTime utcNow)
    {
        if (LockoutEndUtc.HasValue && LockoutEndUtc.Value <= utcNow)
        {
            FailedAttempts = 0;
            LockoutEndUtc = null;
        }
    }

    /// <summary>
    /// Registra un intento fallido. Al alcanzar <paramref name="maxAttempts"/> bloquea la cuenta
    /// durante <paramref name="lockoutDuration"/>.
    /// </summary>
    public void RegisterFailedAttempt(int maxAttempts, TimeSpan lockoutDuration, DateTime utcNow)
    {
        if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        FailedAttempts++;
        if (FailedAttempts >= maxAttempts)
        {
            LockoutEndUtc = utcNow.Add(lockoutDuration);
        }
    }

    /// <summary>Resetea el estado de seguridad tras un login exitoso.</summary>
    public void RegisterSuccessfulLogin()
    {
        FailedAttempts = 0;
        LockoutEndUtc = null;
    }
}
