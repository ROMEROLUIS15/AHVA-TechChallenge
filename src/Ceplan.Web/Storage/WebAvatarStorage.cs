using Ceplan.Application.Abstractions;

namespace Ceplan.Web.Storage;

/// <summary>
/// Implementación de <see cref="IAvatarStorage"/> que guarda las fotos bajo
/// <c>wwwroot/uploads/avatars</c> (servidas como archivos estáticos). El nombre del archivo
/// se genera en el servidor (id + GUID) para evitar colisiones y path traversal; nunca se
/// usa el nombre que envía el cliente.
/// </summary>
public sealed class WebAvatarStorage : IAvatarStorage
{
    private const string RelativeDir = "uploads/avatars";
    private readonly IWebHostEnvironment _env;

    public WebAvatarStorage(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveAsync(int userId, Stream content, string extension, CancellationToken cancellationToken)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
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

        var fullPath = Path.Combine(_env.WebRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
