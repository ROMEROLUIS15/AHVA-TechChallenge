using Ceplan.Application.Abstractions;

namespace Ceplan.Tests.Fakes;

/// <summary>Almacenamiento de avatares en memoria para pruebas (no toca el disco).</summary>
public sealed class FakeAvatarStorage : IAvatarStorage
{
    public int SaveCallCount { get; private set; }
    public string? DeletedPath { get; private set; }

    public Task<string> SaveAsync(int userId, Stream content, string extension, CancellationToken cancellationToken)
    {
        SaveCallCount++;
        return Task.FromResult($"/uploads/avatars/{userId}_fake{extension}");
    }

    public void Delete(string webPath) => DeletedPath = webPath;
}
