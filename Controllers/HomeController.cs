using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.ViewModels;
using System.Diagnostics;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener rutas disponibles para mostrar en la página principal
                var rutasDisponibles = await _context.Rutas
                    .Where(r => r.Activo)
                    .OrderBy(r => r.CiudadOrigen)
                    .Take(6)
                    .ToListAsync();

                ViewBag.RutasDisponibles = rutasDisponibles;

                // Obtener próximos viajes
                var viajesProximos = await _context.Viajes
                    .Include(v => v.Ruta)
                    .Include(v => v.Autobus)
                    .Where(v => v.FechaSalida >= DateTime.Now &&
                               v.Estado == "Programado" &&
                               v.AsientosDisponibles > 0)
                    .OrderBy(v => v.FechaSalida)
                    .Take(8)
                    .ToListAsync();

                ViewBag.ViajesProximos = viajesProximos;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos de la página principal");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarViajes(string origen, string destino, DateTime? fecha)
        {
            try
            {
                var query = _context.Viajes
                    .Include(v => v.Ruta)
                    .Include(v => v.Autobus)
                    .Where(v => v.Estado == "Programado" && v.AsientosDisponibles > 0);

                // Filtrar por origen y destino
                if (!string.IsNullOrEmpty(origen))
                {
                    query = query.Where(v => v.Ruta.CiudadOrigen.ToLower().Contains(origen.ToLower()));
                }

                if (!string.IsNullOrEmpty(destino))
                {
                    query = query.Where(v => v.Ruta.CiudadDestino.ToLower().Contains(destino.ToLower()));
                }

                // Filtrar por fecha si se especifica
                if (fecha.HasValue)
                {
                    var fechaBusqueda = fecha.Value.Date;
                    query = query.Where(v => v.FechaSalida.Date == fechaBusqueda);
                }
                else
                {
                    // Si no se especifica fecha, mostrar desde hoy
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar viajes");
                TempData["Error"] = "Error al realizar la búsqueda";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}