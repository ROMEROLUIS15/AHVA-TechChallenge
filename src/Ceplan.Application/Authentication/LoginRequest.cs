using Ceplan.Domain.Enums;

namespace Ceplan.Application.Authentication;

/// <summary>Datos de entrada de un intento de login.</summary>
public sealed record LoginRequest(DocumentType DocumentType, string DocumentNumber, string Password);
