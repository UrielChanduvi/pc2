using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models;

public class Reserva
{
    public int Id { get; set; }

    [Required]
    public int InmuebleId { get; set; }
    public Inmueble? Inmueble { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required]
    public DateTime FechaExpiracion { get; set; }

    [Required]
    public DateTime FechaCreacion { get; set; }
}
