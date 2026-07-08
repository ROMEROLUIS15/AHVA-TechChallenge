using Ceplan.Application.Abstractions;
using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ceplan.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones pendientes y siembra el usuario del diseño si la tabla está vacía.
/// La contraseña se recibe en claro y se persiste hasheada (nunca se guarda en claro).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        string seedPassword,
        CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var user = User.Create(
            documentType: DocumentType.Dni,
            documentNumber: "07079879",
            passwordHash: passwordHasher.Hash(seedPassword),
            firstName: "July Camila",
            firstLastName: "Mendoza",
            secondLastName: "Quispe",
            role: "Administrador de Recursos",
            organization: "011 Ministerio de Salud",
            birthDate: new DateOnly(1944, 4, 1),
            nationality: "Peruana",
            sex: "Femenino",
            primaryEmail: "test@minsa.gob.pe",
            contractType: "CAS",
            contractDate: new DateOnly(2015, 3, 9),
            isActive: true,
            mobilePhone: "+51 999 999 999");

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
    }
}
