namespace Ceplan.Application.Options;

/// <summary>
/// Parámetros del timeout de sesión por inactividad. Se enlazan desde appsettings (sección "Session").
/// Valores del diseño: 20 min de inactividad (1200 s) y 49 s de cuenta regresiva.
/// Por defecto se usan valores cortos para poder demostrarlo en el video.
/// </summary>
public sealed class SessionTimeoutOptions
{
    public const string SectionName = "Session";

    /// <summary>Segundos de inactividad antes de mostrar el aviso.</summary>
    public int InactivitySeconds { get; set; } = 30;

    /// <summary>Segundos de la cuenta regresiva del aviso antes de expirar.</summary>
    public int CountdownSeconds { get; set; } = 15;
}
