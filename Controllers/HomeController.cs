using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.Entities;
using System.Diagnostics;
using AgenciaDeTransporteWeb.Models;
using AgenciaDeTransporteWeb.Models.ViewModels;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener rutas disponibles y viajes próximos
            var rutasDisponibles = await _context.Rutas
                .Where(r => r.Activo)
                .Take(6)
                .ToListAsync();

            ViewBag.RutasDisponibles = rutasDisponibles;

            // Obtener viajes próximos (próximas 48 horas)
            var viajesProximos = await _context.Viajes
                .Include(v => v.Ruta)
                .Include(v => v.Autobus)
                .Where(v => v.FechaSalida >= DateTime.Now &&
                           v.FechaSalida <= DateTime.Now.AddHours(48) &&
                           v.Estado == "Programado" &&
                           v.AsientosDisponibles > 0)
                .OrderBy(v => v.FechaSalida)
                .Take(8)
                .ToListAsync();

            ViewBag.ViajesProximos = viajesProximos;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> BuscarViajes(string origen, string destino, DateTime? fecha)
        {
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                TempData["Error"] = "Debe seleccionar origen y destino";
                return RedirectToAction("Index");
            }

            var query = _context.Viajes
                .Include(v => v.Ruta)
                .Include(v => v.Autobus)
                .Where(v => v.Ruta.CiudadOrigen.Contains(origen) &&
                           v.Ruta.CiudadDestino.Contains(destino) &&
                           v.Estado == "Programado" &&
                           v.AsientosDisponibles > 0);

            if (fecha.HasValue)
            {
                query = query.Where(v => v.FechaSalida.Date == fecha.Value.Date);
            }
            else
            {
                query = query.Where(v => v.FechaSalida >= DateTime.Now);
            }

            var viajes = await query
                .OrderBy(v => v.FechaSalida)
                .ToListAsync();

            ViewBag.Origen = origen;
            ViewBag.Destino = destino;
            ViewBag.Fecha = fecha;

            return View("ResultadosBusqueda", viajes);
        }
    }
}