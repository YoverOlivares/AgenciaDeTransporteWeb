using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class RutasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RutasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rutas = await _context.Rutas
                .Where(r => r.Activo)
                .ToListAsync();
            return View(rutas);
        }

        public async Task<IActionResult> Detalles(int id)
        {
            var ruta = await _context.Rutas
                .FirstOrDefaultAsync(r => r.Id == id);

            if (ruta == null)
                return NotFound();

            // Obtener viajes disponibles para esta ruta
            var viajes = await _context.Viajes
                .Include(v => v.Autobus)
                .Where(v => v.RutaId == id &&
                           v.FechaSalida >= DateTime.Now &&
                           v.Estado == "Programado" &&
                           v.AsientosDisponibles > 0)
                .OrderBy(v => v.FechaSalida)
                .ToListAsync();

            ViewBag.Viajes = viajes;
            return View(ruta);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ruta ruta)
        {
            if (ModelState.IsValid)
            {
                _context.Rutas.Add(ruta);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ruta creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(ruta);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var ruta = await _context.Rutas.FindAsync(id);
            if (ruta == null)
                return NotFound();

            return View(ruta);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ruta ruta)
        {
            if (id != ruta.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ruta);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ruta actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RutaExists(ruta.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ruta);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ruta = await _context.Rutas
                .FirstOrDefaultAsync(r => r.Id == id);
            if (ruta == null)
                return NotFound();

            return View(ruta);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ruta = await _context.Rutas.FindAsync(id);
            if (ruta != null)
            {
                ruta.Activo = false; // Soft delete
                await _context.SaveChangesAsync();
                TempData["Success"] = "Ruta eliminada exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RutaExists(int id)
        {
            return _context.Rutas.Any(e => e.Id == id);
        }
    }
}