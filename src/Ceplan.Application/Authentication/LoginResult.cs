namespace Ceplan.Application.Authentication;

/// <summary>Posibles resultados de un intento de login.</summary>
public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    ValidationError
}

/// <summary>
/// Resultado tipado del caso de uso de login. Hace explícito el flujo de control
/// (sin usar excepciones para el flujo normal) y el controller lo mapea a vista/mensaje.
/// </summary>
public sealed class LoginResult
{
    private LoginResult(LoginStatus status, AuthenticatedUser? user, DateTime? lockoutEndUtc, string? message)
    {
        Status = status;
        User = user;
        LockoutEndUtc = lockoutEndUtc;
        Message = message;
    }

    public LoginStatus Status { get; }
    public AuthenticatedUser? User { get; }
    public DateTime? LockoutEndUtc { get; }
    public string? Message { get; }

    public bool IsSuccess => Status == LoginStatus.Success;

    public static LoginResult Success(AuthenticatedUser user) =>
        new(LoginStatus.Success, user, null, null);

    public static LoginResult InvalidCredentials() =>
        new(LoginStatus.InvalidCredentials, null, null, null);

    public static LoginResult AccountLocked(DateTime lockoutEndUtc) =>
        new(LoginStatus.AccountLocked, null, lockoutEndUtc, null);

    public static LoginResult ValidationError(string message) =>
        new(LoginStatus.ValidationError, null, null, message);
}
