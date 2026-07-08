using Ceplan.Application.Profile;
using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;
using Ceplan.Infrastructure.Persistence;
using Ceplan.Infrastructure.Storage;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ceplan.IntegrationTests;

/// <summary>
/// Pruebas de integración de la foto de perfil: componentes reales trabajando juntos
/// —EF Core sobre SQLite en memoria, <see cref="EfUserRepository"/> y
/// <see cref="FileSystemAvatarStorage"/> contra el sistema de archivos (carpeta temporal)—.
/// A diferencia de las unitarias, aquí NO hay dobles: se escribe un archivo real y se
/// persiste en una base real.
/// </summary>
public sealed class AvatarUploadIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly string _tempRoot;

    public AvatarUploadIntegrationTests()
    {
        // SQLite en memoria: la BD vive mientras la conexión esté abierta.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using (var db = new AppDbContext(_options))
        {
            db.Database.EnsureCreated();
        }
        _tempRoot = Path.Combine(Path.GetTempPath(), "ceplan-itests-" + Guid.NewGuid().ToString("N"));
    }

    private AppDbContext NewContext() => new(_options);

    private static User NewUser() => User.Create(
        DocumentType.Dni, "07079879", "hash",
        "July Camila", "Mendoza", "Quispe", "Administrador de Recursos", "011 Ministerio de Salud",
        new DateOnly(1990, 1, 1), "Peruana", "Femenino", "test@minsa.gob.pe", "CAS", new DateOnly(2020, 1, 1));

    private static AvatarUpload Png(int bytes = 128) =>
        new(new MemoryStream(new byte[bytes]), "foto.png", bytes, "image/png");

    private string FullPathOf(string webPath) =>
        Path.Combine(_tempRoot, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    [Fact]
    public async Task Upload_ImagenValida_EscribeArchivoRealYPersisteLaRutaEnLaBD()
    {
        int userId;
        await using (var db = NewContext())
        {
            var user = NewUser();
            db.Users.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;
        }

        var storage = new FileSystemAvatarStorage(_tempRoot);
        string? avatarPath;
        await using (var db = NewContext())
        {
            var sut = new ProfileImageService(new EfUserRepository(db), storage);
            var result = await sut.UpdateAsync(userId, Png(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            avatarPath = result.AvatarPath;
            Assert.True(File.Exists(FullPathOf(avatarPath!))); // archivo físico escrito
        }

        // La ruta quedó persistida (se lee desde un contexto nuevo, sin caché).
        await using (var db = NewContext())
        {
            var reloaded = await db.Users.FindAsync(userId);
            Assert.NotNull(reloaded);
            Assert.Equal(avatarPath, reloaded!.AvatarPath);
        }
    }

    [Fact]
    public async Task Upload_Reemplazo_PersisteLaNuevaYBorraElArchivoAnterior()
    {
        int userId;
        await using (var db = NewContext())
        {
            var user = NewUser();
            db.Users.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;
        }

        var storage = new FileSystemAvatarStorage(_tempRoot);

        string firstPath;
        await using (var db = NewContext())
        {
            var sut = new ProfileImageService(new EfUserRepository(db), storage);
            firstPath = (await sut.UpdateAsync(userId, Png(), CancellationToken.None)).AvatarPath!;
        }
        Assert.True(File.Exists(FullPathOf(firstPath)));

        await using (var db = NewContext())
        {
            var sut = new ProfileImageService(new EfUserRepository(db), storage);
            var second = await sut.UpdateAsync(userId, Png(), CancellationToken.None);
            Assert.True(second.IsSuccess);
            Assert.NotEqual(firstPath, second.AvatarPath);
        }

        Assert.False(File.Exists(FullPathOf(firstPath))); // la imagen anterior se eliminó
    }

    [Fact]
    public async Task Storage_SaveYDelete_OperanContraElSistemaDeArchivos()
    {
        var storage = new FileSystemAvatarStorage(_tempRoot);

        var path = await storage.SaveAsync(5, new MemoryStream(new byte[64]), ".png", CancellationToken.None);
        Assert.True(File.Exists(FullPathOf(path)));

        storage.Delete(path);
        Assert.False(File.Exists(FullPathOf(path)));
    }

    public void Dispose()
    {
        _connection.Dispose();
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }
}
