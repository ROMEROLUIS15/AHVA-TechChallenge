using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;

namespace Ceplan.Tests.Fakes;

/// <summary>Fábrica de usuarios de prueba con valores por defecto razonables.</summary>
public static class TestUser
{
    public const string DocumentNumber = "07079879";
    public const string Password = "Ceplan2025$";

    /// <summary>
    /// Crea un usuario del dominio. El "hash" es la contraseña en claro para casar con
    /// <see cref="FakePasswordHasher"/> (hasher invertible de pruebas).
    /// </summary>
    public static User Create(
        DocumentType documentType = DocumentType.Dni,
        string documentNumber = DocumentNumber,
        string password = Password,
        bool isActive = true)
    {
        return User.Create(
            documentType: documentType,
            documentNumber: documentNumber,
            passwordHash: password,
            firstName: "July Camila",
            firstLastName: "Mendoza",
            secondLastName: "Quispe",
            role: "Administrador de Recursos",
            organization: "011 Ministerio de Salud",
            birthDate: new DateOnly(1990, 1, 1),
            nationality: "Peruana",
            sex: "Femenino",
            primaryEmail: "july.mendoza@example.gob.pe",
            contractType: "CAS",
            contractDate: new DateOnly(2020, 1, 1),
            isActive: isActive);
    }
}
