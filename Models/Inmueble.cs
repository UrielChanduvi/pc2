using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models;

public enum TipoInmueble
{
    Departamento,
    Casa,
    Oficina,
    Local
}

public class Inmueble
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    public string? Imagen { get; set; }

    [Required]
    public TipoInmueble Tipo { get; set; }

    [Required]
    [StringLength(50)]
    public string Ciudad { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Direccion { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Dormitorios debe ser mayor a 0")]
    public int Dormitorios { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Baños no puede ser negativo")]
    public int Banos { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "MetrosCuadrados debe ser mayor a 0")]
    public double MetrosCuadrados { get; set; }

    [Range(1, double.MaxValue, ErrorMessage = "Precio debe ser mayor a 0")]
    public decimal Precio { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Visita>? Visitas { get; set; }
    public ICollection<Reserva>? Reservas { get; set; }
}
