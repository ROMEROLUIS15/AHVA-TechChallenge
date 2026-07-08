namespace Ceplan.Application.Authentication;

/// <summary>Caso de uso de autenticación de usuario.</summary>
public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
