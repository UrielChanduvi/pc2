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
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Broker/Reservas/Activas
        public async Task<IActionResult> Activas()
        {
            var ahora = DateTime.Now;
            var reservas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Where(r => r.FechaExpiracion > ahora)
                .OrderBy(r => r.FechaExpiracion)
                .ToListAsync();
            return View(reservas);
        }

        // POST: Broker/Reservas/Liberar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Liberar(int id)
        {
            var reserva = await _context.Reservas.Include(r => r.Inmueble).FirstOrDefaultAsync(r => r.Id == id);
            if (reserva == null)
                return NotFound();
            reserva.FechaExpiracion = DateTime.Now;
            _context.Update(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction("Activas");
        }
    }
}
