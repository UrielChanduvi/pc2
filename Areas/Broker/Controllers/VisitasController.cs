using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        public VisitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Broker/Visitas/Agenda
        public async Task<IActionResult> Agenda(DateTime? fecha)
        {
            var dia = fecha ?? DateTime.Today;
            var visitas = await _context.Visitas
                .Include(v => v.Inmueble)
                .Where(v => v.FechaInicio.Date == dia.Date)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();
            ViewBag.Fecha = dia;
            return View(visitas);
        }
        // POST: Broker/Visitas/Confirmar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            var visita = await _context.Visitas.Include(v => v.Inmueble).FirstOrDefaultAsync(v => v.Id == id);
            if (visita == null)
                return NotFound();
            visita.Estado = EstadoVisita.Confirmada;
            _context.Update(visita);
            await _context.SaveChangesAsync();
            return RedirectToAction("Agenda", new { fecha = visita.FechaInicio.Date });
        }

        // POST: Broker/Visitas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var visita = await _context.Visitas.Include(v => v.Inmueble).FirstOrDefaultAsync(v => v.Id == id);
            if (visita == null)
                return NotFound();
            visita.Estado = EstadoVisita.Cancelada;
            _context.Update(visita);
            await _context.SaveChangesAsync();
            return RedirectToAction("Agenda", new { fecha = visita.FechaInicio.Date });
        }
    }
}
