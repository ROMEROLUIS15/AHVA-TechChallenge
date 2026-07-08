using Ceplan.Application.Abstractions;
using Ceplan.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Ceplan.Infrastructure.Security;

/// <summary>
/// Adaptador sobre <see cref="PasswordHasher{TUser}"/> de ASP.NET Core (PBKDF2 con salt y formato versionado).
/// El parámetro TUser no se usa para el cómputo; solo lo exige la firma del hasher.
/// </summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(user: null!, password);

    public bool Verify(string passwordHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(user: null!, passwordHash, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
