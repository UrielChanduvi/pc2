using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public InmueblesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Broker/Inmuebles
        public async Task<IActionResult> Index()
        {
            var inmuebles = await _context.Inmuebles.ToListAsync();
            return View(inmuebles);
        }

        // GET: Broker/Inmuebles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Broker/Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                inmueble.Activo = true;
                _context.Inmuebles.Add(inmueble);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // GET: Broker/Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
                return NotFound();
            return View(inmueble);
        }

        // POST: Broker/Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(inmueble);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // POST: Broker/Inmuebles/Activar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null)
                return NotFound();
            inmueble.Activo = !inmueble.Activo;
            _context.Update(inmueble);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
