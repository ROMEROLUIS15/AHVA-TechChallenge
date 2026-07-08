using Ceplan.Application.Abstractions;

namespace Ceplan.Tests.Fakes;

/// <summary>Reloj controlable en pruebas: permite fijar y avanzar el tiempo de forma determinista.</summary>
public sealed class FakeClock : IClock
{
    public FakeClock(DateTime utcNow) => UtcNow = utcNow;

    public DateTime UtcNow { get; private set; }

    /// <summary>Avanza el reloj el intervalo indicado (p. ej. para que expire un bloqueo).</summary>
    public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
}
