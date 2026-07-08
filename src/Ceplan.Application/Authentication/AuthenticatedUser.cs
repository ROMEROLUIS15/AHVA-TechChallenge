using Ceplan.Domain.Entities;

namespace Ceplan.Application.Authentication;

/// <summary>
/// Proyección mínima del usuario autenticado para construir la identidad (claims).
/// No expone el hash ni el estado de seguridad.
/// </summary>
public sealed record AuthenticatedUser(int Id, string DocumentNumber, string FullName, string Role)
{
    public static AuthenticatedUser FromDomain(User user) =>
        new(user.Id, user.DocumentNumber, user.FullName, user.Role);
}
