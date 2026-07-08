namespace Ceplan.Application.Abstractions;

/// <summary>
/// Puerto de almacenamiento de fotos de perfil. La implementación concreta (dónde y cómo
/// se guarda el archivo) vive fuera de Application; aquí solo se define el contrato.
/// </summary>
public interface IAvatarStorage
{
    /// <summary>
    /// Guarda el contenido de la imagen y devuelve la ruta web pública para mostrarla
    /// (p. ej. "/uploads/avatars/1_abc.png").
    /// </summary>
    Task<string> SaveAsync(int userId, Stream content, string extension, CancellationToken cancellationToken);

    /// <summary>Elimina una imagen previamente guardada (por su ruta web). No falla si no existe.</summary>
    void Delete(string webPath);
}
