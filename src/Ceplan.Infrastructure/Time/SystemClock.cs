using Ceplan.Application.Abstractions;

namespace Ceplan.Infrastructure.Time;

/// <summary>Reloj real del sistema (UTC).</summary>
public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
