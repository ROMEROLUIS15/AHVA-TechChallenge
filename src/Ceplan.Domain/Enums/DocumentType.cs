namespace Ceplan.Domain.Enums;

/// <summary>
/// Tipo de documento de identidad soportado por el login, según el diseño (toggle DNI / CE).
/// </summary>
public enum DocumentType
{
    /// <summary>Documento Nacional de Identidad (8 dígitos).</summary>
    Dni = 1,

    /// <summary>Carné de Extranjería.</summary>
    Ce = 2
}
