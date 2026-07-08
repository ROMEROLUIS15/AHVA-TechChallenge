namespace Ceplan.Application.Profile;

/// <summary>Caso de uso: cargar/actualizar la foto de perfil de un usuario.</summary>
public interface IProfileImageService
{
    Task<AvatarUpdateResult> UpdateAsync(int userId, AvatarUpload upload, CancellationToken cancellationToken);
}
