using System.Text;
using Ceplan.Application.Profile;
using Ceplan.Tests.Fakes;
using Xunit;

namespace Ceplan.Tests;

/// <summary>
/// Pruebas del caso de uso de foto de perfil: valida tamaño y formato antes de tocar el
/// almacenamiento, y actualiza al usuario solo cuando la imagen es válida. Usa dobles en
/// memoria (repositorio y almacenamiento), sin disco ni BD.
/// </summary>
public sealed class ProfileImageServiceTests
{
    private const int UserId = 0; // El usuario de prueba tiene Id 0 (EF asignaría el real).

    private readonly FakeAvatarStorage _storage = new();

    private static Stream Bytes(int n = 10) => new MemoryStream(Encoding.ASCII.GetBytes(new string('x', n)));

    private ProfileImageService CreateSut(FakeUserRepository users) => new(users, _storage);

    [Fact]
    public async Task Update_ConImagenValida_GuardaYActualizaElUsuario()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);
        var upload = new AvatarUpload(Bytes(), "foto.png", Length: 500_000, ContentType: "image/png");

        var result = await sut.UpdateAsync(UserId, upload, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("/uploads/avatars/0_fake.png", result.AvatarPath);
        Assert.Equal(1, _storage.SaveCallCount);
        Assert.Equal(1, users.SaveChangesCallCount);
    }

    [Fact]
    public async Task Update_ImagenDemasiadoGrande_FallaSinGuardar()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);
        var upload = new AvatarUpload(Bytes(), "foto.png", Length: 2 * 1024 * 1024 + 1, ContentType: "image/png");

        var result = await sut.UpdateAsync(UserId, upload, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, _storage.SaveCallCount); // no debe tocar el almacenamiento
    }

    [Theory]
    [InlineData("archivo.txt", "text/plain")]
    [InlineData("script.svg", "image/svg+xml")] // extensión no permitida
    [InlineData("foto.png", "application/octet-stream")] // content-type no es imagen
    public async Task Update_FormatoNoValido_Falla(string fileName, string contentType)
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);
        var upload = new AvatarUpload(Bytes(), fileName, Length: 1000, ContentType: contentType);

        var result = await sut.UpdateAsync(UserId, upload, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, _storage.SaveCallCount);
    }

    [Fact]
    public async Task Update_ArchivoVacio_Falla()
    {
        var users = new FakeUserRepository(TestUser.Create());
        var sut = CreateSut(users);
        var upload = new AvatarUpload(Bytes(0), "foto.png", Length: 0, ContentType: "image/png");

        var result = await sut.UpdateAsync(UserId, upload, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Update_UsuarioInexistente_Falla()
    {
        var users = new FakeUserRepository(); // sin usuarios
        var sut = CreateSut(users);
        var upload = new AvatarUpload(Bytes(), "foto.png", Length: 1000, ContentType: "image/png");

        var result = await sut.UpdateAsync(userId: 999, upload, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
