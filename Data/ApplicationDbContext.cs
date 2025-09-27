using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PortalInmobiliario.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<PortalInmobiliario.Models.Inmueble> Inmuebles { get; set; }
    public DbSet<PortalInmobiliario.Models.Visita> Visitas { get; set; }
    public DbSet<PortalInmobiliario.Models.Reserva> Reservas { get; set; }
}
