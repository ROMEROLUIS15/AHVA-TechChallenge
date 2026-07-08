namespace Ceplan.Application.Abstractions;

/// <summary>
/// Puerto para el hashing y verificación de contraseñas. La implementación concreta
/// (PBKDF2) vive en Infrastructure; Application no conoce el algoritmo.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera el hash de una contraseña en claro.</summary>
    string Hash(string password);

    /// <summary>Verifica una contraseña en claro contra su hash almacenado.</summary>
    bool Verify(string passwordHash, string providedPassword);
}
