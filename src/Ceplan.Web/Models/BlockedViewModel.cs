namespace Ceplan.Web.Models;

/// <summary>Datos para la pantalla "Cuenta bloqueada temporalmente".</summary>
public sealed class BlockedViewModel
{
    public BlockedViewModel(int maxAttempts, int lockoutMinutes)
    {
        MaxAttempts = maxAttempts;
        LockoutMinutes = lockoutMinutes;
    }

    public int MaxAttempts { get; }
    public int LockoutMinutes { get; }
}
