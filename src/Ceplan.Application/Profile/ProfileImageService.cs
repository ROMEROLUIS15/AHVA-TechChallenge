using Ceplan.Application.Abstractions;

namespace Ceplan.Application.Profile;

/// <summary>
/// Valida y persiste la foto de perfil: comprueba tamaño y formato, delega el guardado
/// del archivo en <see cref="IAvatarStorage"/> y actualiza la ruta en el usuario.
/// No conoce ASP.NET ni el sistema de archivos concreto (depende de puertos, SOLID/DIP).
/// </summary>
public sealed class ProfileImageService : IProfileImageService
{
    private const long MaxBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IUserRepository _users;
    private readonly IAvatarStorage _storage;

    public ProfileImageService(IUserRepository users, IAvatarStorage storage)
    {
        _users = users;
        _storage = storage;
    }

    public async Task<AvatarUpdateResult> UpdateAsync(int userId, AvatarUpload upload, CancellationToken cancellationToken)
    {
        if (upload.Length <= 0)
        {
            return AvatarUpdateResult.Failure("Seleccione una imagen.");
        }
        if (upload.Length > MaxBytes)
        {
            return AvatarUpdateResult.Failure("La imagen no debe superar los 2 MB.");
        }

        var extension = Path.GetExtension(upload.FileName);
        var looksLikeImage = upload.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension) || !looksLikeImage)
        {
            return AvatarUpdateResult.Failure("Formato no válido. Use una imagen JPG, PNG o WEBP.");
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return AvatarUpdateResult.Failure("Usuario no encontrado.");
        }

        var previousPath = user.AvatarPath;
        var newPath = await _storage.SaveAsync(userId, upload.Content, extension, cancellationToken);

        user.SetAvatar(newPath);
        await _users.SaveChangesAsync(cancellationToken);

        // Limpia la imagen anterior una vez persistida la nueva (no crítico si falla).
        if (!string.IsNullOrEmpty(previousPath))
        {
            _storage.Delete(previousPath);
        }

        return AvatarUpdateResult.Success(newPath);
    }
}
