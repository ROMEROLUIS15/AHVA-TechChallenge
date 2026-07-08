using Ceplan.Application.Abstractions;
using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;

namespace Ceplan.Tests.Fakes;

/// <summary>
/// Repositorio en memoria para pruebas. Guarda un conjunto de usuarios y cuenta las
/// persistencias, de modo que los tests puedan afirmar que el estado de seguridad se guardó.
/// </summary>
public sealed class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public int SaveChangesCallCount { get; private set; }

    public FakeUserRepository(params User[] users) => _users.AddRange(users);

    public Task<User?> GetByDocumentAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.DocumentType == documentType && u.DocumentNumber == documentNumber));

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
