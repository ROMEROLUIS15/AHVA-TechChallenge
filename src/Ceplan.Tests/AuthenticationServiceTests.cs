using Ceplan.Application.Authentication;
using Ceplan.Application.Options;
using Ceplan.Domain.Enums;
using Ceplan.Tests.Fakes;
using Xunit;

namespace Ceplan.Tests;

/// <summary>
/// Pruebas de la lógica evaluada: validación de credenciales, contador de fallos (CVF) y
/// bloqueo temporal. Usan <see cref="FakeClock"/> para hacer el tiempo determinista, de modo
/// que el bloqueo y su expiración se prueban sin esperas reales.
/// </summary>
public sealed class AuthenticationServiceTests
{
    private static readonly DateTime Now = new(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeClock _clock = new(Now);
    private readonly FakePasswordHasher _hasher = new();
    private readonly LockoutOptions _lockout = new() { MaxFailedAttempts = 5, LockoutMinutes = 15 };

    private AuthenticationService CreateSut(FakeUserRepository users) =>
        new(users, _hasher, _clock, _lockout);

    private static LoginRequest Request(string password, DocumentType type = DocumentType.Dni) =>
        new(type, TestUser.DocumentNumber, password);

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveSuccess()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        var result = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.User);
        Assert.Equal(TestUser.DocumentNumber, result.User!.DocumentNumber);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_DevuelveInvalidCredentials()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        var result = await sut.LoginAsync(Request("password-incorrecta"), CancellationToken.None);

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_DevuelveInvalidCredentials()
    {
        var users = new FakeUserRepository(); // sin usuarios
        var sut = CreateSut(users);

        var result = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);

        // Mensaje genérico: no revela si el usuario existe.
        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
    }

    [Fact]
    public async Task Login_UsuarioInactivo_DevuelveInvalidCredentials()
    {
        var users = new FakeUserRepository(TestUser.Create(isActive: false));
        var sut = CreateSut(users);

        var result = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Login_SinCredenciales_DevuelveValidationError(string password)
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        var result = await sut.LoginAsync(Request(password), CancellationToken.None);

        Assert.Equal(LoginStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task Login_AlQuintoIntentoFallido_BloqueaLaCuenta()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        // Intentos 1-4: siguen como credenciales inválidas (aún no bloqueado).
        for (var i = 0; i < _lockout.MaxFailedAttempts - 1; i++)
        {
            var partial = await sut.LoginAsync(Request("mala"), CancellationToken.None);
            Assert.Equal(LoginStatus.InvalidCredentials, partial.Status);
        }

        // 5.º intento: la cuenta queda bloqueada.
        var result = await sut.LoginAsync(Request("mala"), CancellationToken.None);

        Assert.Equal(LoginStatus.AccountLocked, result.Status);
        Assert.Equal(Now.AddMinutes(_lockout.LockoutMinutes), result.LockoutEndUtc);
    }

    [Fact]
    public async Task Login_CuentaBloqueada_RechazaSinValidarPassword()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        // Provoca el bloqueo con 5 fallos.
        for (var i = 0; i < _lockout.MaxFailedAttempts; i++)
        {
            await sut.LoginAsync(Request("mala"), CancellationToken.None);
        }

        var verifyCountTrasBloqueo = _hasher.VerifyCallCount;

        // Nuevo intento, incluso con la contraseña correcta, sigue bloqueado...
        var result = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);

        Assert.Equal(LoginStatus.AccountLocked, result.Status);
        // ...y NO se validó la contraseña (no se llamó al hasher otra vez).
        Assert.Equal(verifyCountTrasBloqueo, _hasher.VerifyCallCount);
    }

    [Fact]
    public async Task Login_TrasExpirarElBloqueo_PermiteIngresarYReseteaElContador()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        // Bloquea la cuenta.
        for (var i = 0; i < _lockout.MaxFailedAttempts; i++)
        {
            await sut.LoginAsync(Request("mala"), CancellationToken.None);
        }

        // Avanza el reloj más allá de la ventana de bloqueo.
        _clock.Advance(TimeSpan.FromMinutes(_lockout.LockoutMinutes + 1));

        // Con la contraseña correcta, el login vuelve a ser exitoso.
        var result = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);

        Assert.Equal(LoginStatus.Success, result.Status);
    }

    [Fact]
    public async Task Login_ExitosoTrasFallosPrevios_ReseteaElContadorDeFallos()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);

        // Dos fallos (por debajo del umbral).
        await sut.LoginAsync(Request("mala"), CancellationToken.None);
        await sut.LoginAsync(Request("mala"), CancellationToken.None);

        // Login correcto: resetea el contador.
        var ok = await sut.LoginAsync(Request(TestUser.Password), CancellationToken.None);
        Assert.Equal(LoginStatus.Success, ok.Status);

        // Ahora cuatro fallos seguidos NO deben bloquear (el contador partió de cero):
        // solo el 5.º consecutivo bloquearía.
        for (var i = 0; i < _lockout.MaxFailedAttempts - 1; i++)
        {
            var partial = await sut.LoginAsync(Request("mala"), CancellationToken.None);
            Assert.Equal(LoginStatus.InvalidCredentials, partial.Status);
        }
    }
}
