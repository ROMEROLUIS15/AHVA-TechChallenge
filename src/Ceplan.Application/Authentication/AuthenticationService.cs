using Ceplan.Application.Abstractions;
using Ceplan.Application.Options;

namespace Ceplan.Application.Authentication;

/// <summary>
/// Orquesta la validación de credenciales y la política de bloqueo por intentos fallidos.
/// No conoce EF Core, hashing concreto ni el reloj real: depende de puertos (SOLID/DIP).
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IClock _clock;
    private readonly LockoutOptions _lockout;

    public AuthenticationService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IClock clock,
        LockoutOptions lockout)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _clock = clock;
        _lockout = lockout;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Validación defensiva (el controller también valida en el borde).
        if (string.IsNullOrWhiteSpace(request.DocumentNumber) || string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginResult.ValidationError("Debe ingresar usuario y contraseña.");
        }

        var user = await _users.GetByDocumentAsync(request.DocumentType, request.DocumentNumber.Trim(), cancellationToken);

        // Usuario inexistente o inactivo: mensaje genérico que no revela si el usuario existe.
        if (user is null || !user.IsActive)
        {
            return LoginResult.InvalidCredentials();
        }

        var now = _clock.UtcNow;

        // Si un bloqueo previo ya expiró, se limpia antes de evaluar.
        user.ClearExpiredLockout(now);

        // Cuenta bloqueada: se rechaza sin validar credenciales.
        if (user.IsLockedOut(now))
        {
            return LoginResult.AccountLocked(user.LockoutEndUtc!.Value);
        }

        var passwordValid = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!passwordValid)
        {
            user.RegisterFailedAttempt(_lockout.MaxFailedAttempts, TimeSpan.FromMinutes(_lockout.LockoutMinutes), now);
            await _users.SaveChangesAsync(cancellationToken);

            return user.IsLockedOut(now)
                ? LoginResult.AccountLocked(user.LockoutEndUtc!.Value)
                : LoginResult.InvalidCredentials();
        }

        // Éxito: se resetea el contador de intentos.
        user.RegisterSuccessfulLogin();
        await _users.SaveChangesAsync(cancellationToken);

        return LoginResult.Success(AuthenticatedUser.FromDomain(user));
    }
}
