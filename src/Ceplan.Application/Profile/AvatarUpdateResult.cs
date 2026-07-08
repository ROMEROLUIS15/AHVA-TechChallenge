namespace Ceplan.Application.Profile;

/// <summary>
/// Resultado tipado de actualizar la foto de perfil. Hace explícito el flujo de control
/// (sin excepciones para errores esperados de validación) y el controller lo mapea a mensaje.
/// </summary>
public sealed class AvatarUpdateResult
{
    private AvatarUpdateResult(bool isSuccess, string? avatarPath, string? error)
    {
        IsSuccess = isSuccess;
        AvatarPath = avatarPath;
        Error = error;
    }

    public bool IsSuccess { get; }
    public string? AvatarPath { get; }
    public string? Error { get; }

    public static AvatarUpdateResult Success(string avatarPath) => new(true, avatarPath, null);
    public static AvatarUpdateResult Failure(string error) => new(false, null, error);
}
