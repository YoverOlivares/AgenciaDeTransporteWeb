// Controllers/ReservasController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Models.ViewModels;

namespace AgenciaDeTransporteWeb.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            ILogger<ReservasController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var reservas = await _context.Reservas
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Autobus)
                .Include(r => r.Asiento)
                .Where(r => r.UsuarioId == usuario.Id)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            return View(reservas);
        }

        public async Task<IActionResult> SeleccionarAsiento(int viajeId)
        {
            var viaje = await _context.Viajes
                .Include(v => v.Ruta)
                .Include(v => v.Autobus)
                    .ThenInclude(a => a.Asientos)
                .FirstOrDefaultAsync(v => v.Id == viajeId);

            if (viaje == null)
            {
                TempData["Error"] = "Viaje no encontrado";
                return RedirectToAction("Index", "Home");
            }

            if (viaje.AsientosDisponibles <= 0)
            {
                TempData["Error"] = "No hay asientos disponibles para este viaje";
                return RedirectToAction("Index", "Home");
            }

            // Obtener asientos ya reservados
            var asientosReservados = await _context.Reservas
                .Where(r => r.ViajeId == viajeId && r.EstadoReserva != "Cancelada")
                .Select(r => r.AsientoId)
                .ToListAsync();

            // Filtrar asientos disponibles
            var asientosDisponibles = viaje.Autobus.Asientos
                .Where(a => a.Activo && !asientosReservados.Contains(a.Id))
                .OrderBy(a => a.NumeroAsiento)
                .ToList();

            var viewModel = new SeleccionAsientoViewModel
            {
                Viaje = viaje,
                AsientosDisponibles = asientosDisponibles
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CrearReserva(int viajeId, int asientoId)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                    return RedirectToAction("Login", "Account");

                // Validar que el asiento esté disponible
                var asientoDisponible = await _context.Reservas
                    .AnyAsync(r => r.AsientoId == asientoId &&
                                  r.ViajeId == viajeId &&
                                  r.EstadoReserva != "Cancelada");

                if (asientoDisponible)
                {
                    TempData["Error"] = "El asiento seleccionado ya no está disponible";
                    return RedirectToAction("SeleccionarAsiento", new { viajeId });
                }

                var viaje = await _context.Viajes
                    .Include(v => v.Ruta)
                    .FirstOrDefaultAsync(v => v.Id == viajeId);

                if (viaje == null)
                {
                    TempData["Error"] = "Viaje no encontrado";
                    return RedirectToAction("Index", "Home");
                }

                // Generar código único de reserva
                var codigoReserva = $"RES{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";

                var reserva = new Reserva
                {
                    UsuarioId = usuario.Id,
                    ViajeId = viajeId,
                    AsientoId = asientoId,
                    CodigoReserva = codigoReserva,
                    FechaReserva = DateTime.Now,
                    EstadoReserva = "Pendiente",
                    MontoTotal = viaje.PrecioViaje
                };

                _context.Reservas.Add(reserva);

                // Actualizar asientos disponibles
                viaje.AsientosDisponibles--;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Reserva creada exitosamente. Código: {codigoReserva}";
                return RedirectToAction("Detalles", new { codigo = codigoReserva });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                TempData["Error"] = "Error al crear la reserva. Intente nuevamente.";
                return RedirectToAction("SeleccionarAsiento", new { viajeId });
            }
        }

        public async Task<IActionResult> Detalles(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Autobus)
                .Include(r => r.Asiento)
                .FirstOrDefaultAsync(r => r.CodigoReserva == codigo);

            if (reserva == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || reserva.UsuarioId != usuario.Id)
                return Forbid();

            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                    return RedirectToAction("Login", "Account");

                var reserva = await _context.Reservas
                    .Include(r => r.Viaje)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuario.Id);

                if (reserva == null)
                {
                    TempData["Error"] = "Reserva no encontrada";
                    return RedirectToAction("Index");
                }

                if (reserva.EstadoReserva == "Cancelada" || reserva.EstadoReserva == "Completada")
                {
                    TempData["Error"] = "No se puede cancelar esta reserva";
                    return RedirectToAction("Index");
                }

                reserva.EstadoReserva = "Cancelada";
                reserva.Viaje.AsientosDisponibles++;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Reserva cancelada exitosamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva");
                TempData["Error"] = "Error al cancelar la reserva";
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConsultarReserva(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return View();

            var reserva = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Autobus)
                .Include(r => r.Asiento)
                .FirstOrDefaultAsync(r => r.CodigoReserva == codigo);

            return View(reserva);
        }

        public async Task<IActionResult> Pagar(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Viaje)
                    .ThenInclude(v => v.Ruta)
                .Include(r => r.Asiento)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
                return NotFound();

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null || reserva.UsuarioId != usuario.Id)
                return Forbid();

            if (reserva.EstadoReserva != "Pendiente")
            {
                TempData["Error"] = "Esta reserva no puede ser pagada";
                return RedirectToAction("Detalles", new { codigo = reserva.CodigoReserva });
            }

            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarPago(int reservaId, string metodoPago)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(reservaId);
                if (reserva == null)
                    return NotFound();

                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null || reserva.UsuarioId != usuario.Id)
                    return Forbid();

                // Crear transacción
                var transaccion = new Transaccion
                {
                    ReservaId = reservaId,
                    TipoTransaccion = "Pago",
                    MetodoPago = metodoPago,
                    Monto = reserva.MontoTotal,
                    FechaTransaccion = DateTime.Now,
                    EstadoTransaccion = "Completada",
                    ReferenciaPago = $"PAY{DateTime.Now:yyyyMMddHHmmss}"
                };

                _context.Transacciones.Add(transaccion);

                // Actualizar estado de reserva
                reserva.EstadoReserva = "Confirmada";

                await _context.SaveChangesAsync();

                TempData["Success"] = "Pago procesado exitosamente. Su reserva ha sido confirmada.";
                return RedirectToAction("Detalles", new { codigo = reserva.CodigoReserva });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago");
                TempData["Error"] = "Error al procesar el pago. Intente nuevamente.";
                return RedirectToAction("Pagar", new { id = reservaId });
            }
        }
    }
}