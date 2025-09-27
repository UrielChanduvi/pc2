    public async Task<IActionResult> Detalle(int id)
    {
        var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(i => i.Id == id && i.Activo);
        if (inmueble == null)
            return NotFound();
        return View(inmueble);
    }
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;

namespace PortalInmobiliario.Controllers;

public class CatalogoController : Controller
{
    private readonly ApplicationDbContext _context;
    public CatalogoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1, int pageSize = 5)
    {
        var query = _context.Inmuebles.Where(i => i.Activo);

        List<string> errors = new();
        if (precioMin.HasValue && precioMin < 0)
            errors.Add("Precio mínimo no puede ser negativo.");
        if (precioMax.HasValue && precioMax < 0)
            errors.Add("Precio máximo no puede ser negativo.");
        if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
            errors.Add("Precio mínimo no puede ser mayor que el máximo.");
        if (dormitorios.HasValue && dormitorios < 0)
            errors.Add("Dormitorios no puede ser negativo.");

        if (!string.IsNullOrEmpty(ciudad))
            query = query.Where(i => i.Ciudad == ciudad);
        if (tipo.HasValue)
            query = query.Where(i => i.Tipo == tipo);
        if (precioMin.HasValue && precioMin >= 0)
            query = query.Where(i => i.Precio >= precioMin);
        if (precioMax.HasValue && precioMax >= 0)
            query = query.Where(i => i.Precio <= precioMax);
        if (dormitorios.HasValue && dormitorios >= 0)
            query = query.Where(i => i.Dormitorios >= dormitorios);

        var totalItems = await query.CountAsync();
        var inmuebles = await query
            .OrderBy(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Ciudad = ciudad;
        ViewBag.Tipo = tipo;
        ViewBag.PrecioMin = precioMin;
        ViewBag.PrecioMax = precioMax;
        ViewBag.Dormitorios = dormitorios;
        ViewBag.Ciudades = await _context.Inmuebles.Select(i => i.Ciudad).Distinct().ToListAsync();
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalItems = totalItems;
        ViewBag.Errors = errors;
        return View(inmuebles);
    }
}
