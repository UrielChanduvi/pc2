// ...existing code...
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Data;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        public CatalogoController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index(string? ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1, int pageSize = 5)
        {
                // Clave compuesta por filtros
                string cacheKey = $"inmuebles:{ciudad}:{tipo}:{precioMin}:{precioMax}:{dormitorios}:{page}:{pageSize}";
                var cached = await _cache.GetStringAsync(cacheKey);
                List<PortalInmobiliario.Models.Inmueble>? inmuebles = null;
                int totalItems = 0;
                if (cached != null)
                {
                    inmuebles = System.Text.Json.JsonSerializer.Deserialize<List<PortalInmobiliario.Models.Inmueble>>(cached);
                    totalItems = inmuebles?.Count ?? 0;
                }
                else
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
                    totalItems = await query.CountAsync();
                    inmuebles = await query
                        .OrderBy(i => i.Id)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                    var json = System.Text.Json.JsonSerializer.Serialize(inmuebles);
                    await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) });
                    ViewBag.Errors = errors;
                }
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
        // Guardar filtros en sesión
        HttpContext.Session.SetString("FiltroCiudad", ciudad ?? "");
        HttpContext.Session.SetString("FiltroTipo", tipo?.ToString() ?? "");
        HttpContext.Session.SetString("FiltroPrecioMin", precioMin?.ToString() ?? "");
        HttpContext.Session.SetString("FiltroPrecioMax", precioMax?.ToString() ?? "");
        HttpContext.Session.SetString("FiltroDormitorios", dormitorios?.ToString() ?? "");
        return View(inmuebles);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(i => i.Id == id && i.Activo);
            if (inmueble == null)
                return NotFound();

            // Verificar si hay reserva activa
            var reservaActiva = await _context.Reservas.AnyAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.Now);
            ViewBag.ReservaActiva = reservaActiva;
            // Guardar último inmueble visitado en sesión
            HttpContext.Session.SetInt32("UltimoInmuebleId", id);
            return View(inmueble);
        }

        [HttpPost]
        public async Task<IActionResult> AgendarVisita(int InmuebleId, DateTime FechaInicio, DateTime FechaFin, string? Notas)
        {
            // Validar usuario autenticado
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["Error"] = "Debes iniciar sesión para agendar una visita.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Validar fechas
            if (FechaInicio >= FechaFin)
            {
                TempData["Error"] = "La fecha de inicio debe ser menor que la fecha de fin.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Validar horario laboral (08:00–19:00)
            if (FechaInicio.Hour < 8 || FechaFin.Hour > 19)
            {
                TempData["Error"] = "Las visitas solo pueden agendarse entre 08:00 y 19:00.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Validar solapamiento de visitas
            var solapada = await _context.Visitas.AnyAsync(v => v.InmuebleId == InmuebleId &&
                ((FechaInicio < v.FechaFin && FechaFin > v.FechaInicio)));
            if (solapada)
            {
                TempData["Error"] = "Ya existe una visita agendada en ese intervalo.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Crear visita
            var visita = new PortalInmobiliario.Models.Visita
            {
                InmuebleId = InmuebleId,
                UsuarioId = User.Identity?.Name ?? "",
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Estado = PortalInmobiliario.Models.EstadoVisita.Solicitada,
                Notas = Notas
            };
            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();
            // Invalidar caché de inmuebles activos
            await InvalidarCacheInmuebles();
            TempData["Success"] = "Visita agendada correctamente.";
            return RedirectToAction("Detalle", new { id = InmuebleId });
        }
        // Método para invalidar caché de inmuebles activos
        private async Task InvalidarCacheInmuebles()
        {
            // Elimina todas las claves que empiezan con "inmuebles:"
            // StackExchangeRedisCache no soporta enumerar claves directamente, así que se recomienda usar un prefijo único por filtro
            // Aquí se elimina la clave base sin filtros, pero en producción se recomienda usar un script Redis para eliminar por patrón
            await _cache.RemoveAsync("inmuebles::::::1:5");
        }

        [HttpPost]
        public async Task<IActionResult> ReservarAhora(int InmuebleId)
        {
            // Validar usuario autenticado
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["Error"] = "Debes iniciar sesión para reservar.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Validar reserva activa
            var reservaActiva = await _context.Reservas.AnyAsync(r => r.InmuebleId == InmuebleId && r.FechaExpiracion > DateTime.Now);
            if (reservaActiva)
            {
                TempData["Error"] = "Ya existe una reserva activa para este inmueble.";
                return RedirectToAction("Detalle", new { id = InmuebleId });
            }

            // Crear reserva por 48h
            var reserva = new PortalInmobiliario.Models.Reserva
            {
                InmuebleId = InmuebleId,
                UsuarioId = User.Identity?.Name ?? "",
                FechaCreacion = DateTime.Now,
                FechaExpiracion = DateTime.Now.AddHours(48)
            };
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Reserva creada por 48 horas.";
            return RedirectToAction("Detalle", new { id = InmuebleId });
        }
    }

