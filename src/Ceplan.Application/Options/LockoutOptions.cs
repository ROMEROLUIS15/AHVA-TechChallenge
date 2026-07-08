namespace Ceplan.Application.Options;

/// <summary>
/// Parámetros del bloqueo por intentos fallidos. Se enlazan desde appsettings (sección "Lockout").
/// Valores por defecto según el diseño: 5 intentos, 15 minutos.
/// </summary>
public sealed class LockoutOptions
{
    public const string SectionName = "Lockout";

    /// <summary>Número de intentos fallidos que dispara el bloqueo.</summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>Duración del bloqueo temporal, en minutos.</summary>
    public int LockoutMinutes { get; set; } = 15;
}
