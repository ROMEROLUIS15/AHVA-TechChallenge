using Ceplan.Application.Abstractions;

namespace Ceplan.Infrastructure.Storage;

/// <summary>
/// Guarda las fotos de perfil en el sistema de archivos, bajo <c>{rootPath}/uploads/avatars</c>,
/// y devuelve su ruta web pública. El nombre del archivo se genera en el servidor (id + GUID)
/// para evitar colisiones y path traversal; nunca se usa el nombre que envía el cliente.
/// No depende de ASP.NET: el <c>rootPath</c> (p. ej. wwwroot) se inyecta, lo que la hace
/// testeable de forma aislada.
/// </summary>
public sealed class FileSystemAvatarStorage : IAvatarStorage
{
    private const string RelativeDir = "uploads/avatars";
    private readonly string _rootPath;

    public FileSystemAvatarStorage(string rootPath) => _rootPath = rootPath;

    public async Task<string> SaveAsync(int userId, Stream content, string extension, CancellationToken cancellationToken)
    {
        var dir = Path.Combine(_rootPath, "uploads", "avatars");
        Directory.CreateDirectory(dir);

        var fileName = $"{userId}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var fullPath = Path.Combine(dir, fileName);

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }

        return $"/{RelativeDir}/{fileName}";
    }

    public void Delete(string webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath))
        {
            return;
        }

        // Solo permitimos borrar dentro de la carpeta de avatares (defensa ante rutas raras).
        var relative = webPath.TrimStart('/');
        if (!relative.StartsWith(RelativeDir, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fullPath = Path.Combine(_rootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
