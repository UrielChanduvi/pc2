using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        if (!context.Inmuebles.Any())
        {
            context.Inmuebles.AddRange(
                new Inmueble {
                    Codigo = "DEP001",
                    Titulo = "Departamento céntrico",
                    Imagen = null,
                    Tipo = TipoInmueble.Departamento,
                    Ciudad = "Lima",
                    Direccion = "Av. Principal 123",
                    Dormitorios = 2,
                    Banos = 1,
                    MetrosCuadrados = 65,
                    Precio = 120000,
                    Activo = true
                },
                new Inmueble {
                    Codigo = "CASA002",
                    Titulo = "Casa familiar",
                    Imagen = null,
                    Tipo = TipoInmueble.Casa,
                    Ciudad = "Arequipa",
                    Direccion = "Calle Secundaria 45",
                    Dormitorios = 3,
                    Banos = 2,
                    MetrosCuadrados = 120,
                    Precio = 250000,
                    Activo = true
                },
                new Inmueble {
                    Codigo = "OFI003",
                    Titulo = "Oficina moderna",
                    Imagen = null,
                    Tipo = TipoInmueble.Oficina,
                    Ciudad = "Lima",
                    Direccion = "Av. Empresarial 789",
                    Dormitorios = 0,
                    Banos = 2,
                    MetrosCuadrados = 80,
                    Precio = 180000,
                    Activo = true
                },
                new Inmueble {
                    Codigo = "LOC004",
                    Titulo = "Local comercial",
                    Imagen = null,
                    Tipo = TipoInmueble.Local,
                    Ciudad = "Cusco",
                    Direccion = "Jr. Comercio 10",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 40,
                    Precio = 90000,
                    Activo = true
                }
            );
            context.SaveChanges();
        }
    }
}
