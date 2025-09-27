using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models;

public enum EstadoVisita
{
    Solicitada,
    Confirmada,
    Cancelada
}

public class Visita
{
    public int Id { get; set; }

    [Required]
    public int InmuebleId { get; set; }
    public Inmueble? Inmueble { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    [DateGreaterThan("FechaInicio", ErrorMessage = "FechaFin debe ser mayor que FechaInicio")]
    public DateTime FechaFin { get; set; }

    [Required]
    public EstadoVisita Estado { get; set; }

    [StringLength(200)]
    public string? Notas { get; set; }
}

// Custom validation attribute for FechaFin > FechaInicio
public class DateGreaterThanAttribute : ValidationAttribute
{
    private readonly string _otherPropertyName;
    public DateGreaterThanAttribute(string otherPropertyName)
    {
        _otherPropertyName = otherPropertyName;
    }
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var otherProperty = validationContext.ObjectType.GetProperty(_otherPropertyName);
        if (otherProperty == null)
            return new ValidationResult($"Propiedad {_otherPropertyName} no encontrada");
        var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
        if (value is DateTime fechaFin && otherValue is DateTime fechaInicio)
        {
            if (fechaFin <= fechaInicio)
                return new ValidationResult(ErrorMessage);
        }
        return ValidationResult.Success;
    }
}
