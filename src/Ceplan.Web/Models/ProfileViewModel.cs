using Ceplan.Domain.Entities;
using Ceplan.Domain.Enums;

namespace Ceplan.Web.Models;

/// <summary>Proyección de solo lectura del usuario para la vista de perfil.</summary>
public sealed class ProfileViewModel
{
    public required string FullName { get; init; }
    public required string Role { get; init; }
    public required string Organization { get; init; }
    public required bool IsActive { get; init; }

    public required string FirstName { get; init; }
    public required string FirstLastName { get; init; }
    public required string SecondLastName { get; init; }
    public required string DocumentTypeLabel { get; init; }
    public required string DocumentNumber { get; init; }
    public required string BirthDate { get; init; }
    public required string Nationality { get; init; }
    public required string Sex { get; init; }
    public required string PrimaryEmail { get; init; }
    public string? SecondaryEmail { get; init; }
    public string? MobilePhone { get; init; }
    public string? SecondaryPhone { get; init; }
    public required string ContractType { get; init; }
    public required string ContractDate { get; init; }

    public static ProfileViewModel FromUser(User user) => new()
    {
        FullName = user.FullName,
        Role = user.Role,
        Organization = user.Organization,
        IsActive = user.IsActive,
        FirstName = user.FirstName,
        FirstLastName = user.FirstLastName,
        SecondLastName = user.SecondLastName,
        DocumentTypeLabel = user.DocumentType == DocumentType.Dni ? "DNI" : "CE",
        DocumentNumber = user.DocumentNumber,
        BirthDate = user.BirthDate.ToString("dd / MM / yyyy"),
        Nationality = user.Nationality,
        Sex = user.Sex,
        PrimaryEmail = user.PrimaryEmail,
        SecondaryEmail = user.SecondaryEmail,
        MobilePhone = user.MobilePhone,
        SecondaryPhone = user.SecondaryPhone,
        ContractType = user.ContractType,
        ContractDate = user.ContractDate.ToString("dd / MM / yyyy")
    };
}
