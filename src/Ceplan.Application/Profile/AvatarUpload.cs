namespace Ceplan.Application.Profile;

/// <summary>Datos de entrada de una foto de perfil a cargar (independiente de ASP.NET/IFormFile).</summary>
public sealed record AvatarUpload(Stream Content, string FileName, long Length, string ContentType);
