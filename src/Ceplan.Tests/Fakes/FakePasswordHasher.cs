using Ceplan.Application.Abstractions;

namespace Ceplan.Tests.Fakes;

/// <summary>
/// Hasher falso e invertible para pruebas: el "hash" es la propia contraseña, por lo que
/// <see cref="Verify"/> compara por igualdad. Registra si se invocó, para verificar que una
/// cuenta bloqueada se rechaza SIN validar credenciales.
/// </summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCallCount { get; private set; }

    public string Hash(string password) => password;

    public bool Verify(string passwordHash, string providedPassword)
    {
        VerifyCallCount++;
        return passwordHash == providedPassword;
    }
}
