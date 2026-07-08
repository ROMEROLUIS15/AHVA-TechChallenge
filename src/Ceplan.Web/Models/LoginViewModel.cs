using System.ComponentModel.DataAnnotations;
using Ceplan.Domain.Enums;

namespace Ceplan.Web.Models;

/// <summary>Modelo del formulario de login (toggle DNI/CE, usuario, contraseña).</summary>
public sealed class LoginViewModel
{
    public DocumentType DocumentType { get; set; } = DocumentType.Dni;

    [Required(ErrorMessage = "Ingrese su usuario.")]
    [Display(Name = "Usuario")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}
