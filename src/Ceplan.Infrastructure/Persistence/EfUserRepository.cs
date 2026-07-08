using Ceplan.Application.Abstractions;
using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ceplan.Infrastructure.Persistence;

/// <summary>Implementación EF Core del repositorio de usuarios.</summary>
public sealed class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public EfUserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByDocumentAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.DocumentType == documentType && u.DocumentNumber == documentNumber, cancellationToken);

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}
