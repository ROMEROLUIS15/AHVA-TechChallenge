namespace Ceplan.Application.Abstractions;

/// <summary>
/// Abstracción del reloj del sistema. Permite que la lógica de bloqueo sea determinista
/// y testeable (se puede inyectar un reloj falso en pruebas).
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
