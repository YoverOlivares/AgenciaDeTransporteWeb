using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.ViewModels;
using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalReservas = await _context.Reservas.CountAsync(),
                TotalAutobuses = await _context.Autobuses.Where(a => a.Activo).CountAsync(),
                TotalRutas = await _context.Rutas.Where(r => r.Activo).CountAsync(),

                IngresosMensuales = await _context.Transacciones
                    .Where(t => t.FechaTransaccion.Month == DateTime.Now.Month &&
                               t.FechaTransaccion.Year == DateTime.Now.Year &&
                               t.EstadoTransaccion == "Completada")
                    .SumAsync(t => t.Monto),

                ReservasRecientes = await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                    .OrderByDescending(r => r.FechaReserva)
                    .Take(10)
                    .ToListAsync(),

                ViajesProximos = await _context.Viajes
                    .Include(v => v.Ruta)
                    .Include(v => v.Autobus)
                    .Where(v => v.FechaSalida >= DateTime.Now &&
                               v.Estado == "Programado")
                    .OrderBy(v => v.FechaSalida)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GestionarAutobuses()
        {
            var autobuses = await _context.Autobuses
                .Where(a => a.Activo)
                .ToListAsync();
            return View(autobuses);
        }

        public async Task<IActionResult> GestionarRutas()
        {
            var rutas = await _context.Rutas
                .Where(r => r.Activo)
                .ToListAsync();
            return View(rutas);
        }

        public async Task<IActionResult> HistorialTransacciones()
        {
            var transacciones = await _context.Transacciones
                .Include(t => t.Reserva)
                .ThenInclude(r => r.Usuario)
                .OrderByDescending(t => t.FechaTransaccion)
                .Take(100)
                .ToListAsync();
            return View(transacciones);
        }

        public IActionResult Reportes()
        {
            return View();
        }
    }
}