using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;

namespace Ceplan.Application.Abstractions;

/// <summary>
/// Puerto de acceso a usuarios. La implementación (EF Core) vive en Infrastructure.
/// Los métodos devuelven entidades rastreadas por el contexto; <see cref="SaveChangesAsync"/>
/// persiste los cambios (unidad de trabajo simple para este alcance).
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByDocumentAsync(DocumentType documentType, string documentNumber, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
