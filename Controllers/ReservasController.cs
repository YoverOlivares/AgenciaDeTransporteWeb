using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Models.ViewModels;
using AgenciaDeTransporteWeb.Services.Interfaces;

namespace AgenciaDeTransporteWeb.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly ILogger<ReservasController> _logger;
        private readonly IReservaService _reservaService;
        private readonly IPagoService _pagoService;
        private readonly IPDFService _pdfService;

        public ReservasController(
            ApplicationDbContext context,
            UserManager<Usuario> userManager,
            ILogger<ReservasController> logger,
            IReservaService reservaService,
            IPagoService pagoService,
            IPDFService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _reservaService = reservaService;
            _pagoService = pagoService;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return RedirectToAction("Login", "Account");

            var reservas = await _reservaService.ObtenerReservasUsuarioAsync(usuario.Id);
            return View(reservas);
        }

        public async Task<IActionResult> SeleccionarAsiento(int viajeId)
        {
            var viaje = await _context.Viajes
                .Include(v => v.Ruta)
                .Include(v => v.Autobus)
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

            var asientosDisponibles = await _reservaService.ObtenerAsientosDisponiblesAsync(viajeId);

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

                var resultado = await _reservaService.CrearReservaAsync(usuario.Id, viajeId, asientoId);

                if (resultado.Success)
                {
                    TempData["Success"] = $"Reserva creada exitosamente. Código: {resultado.Reserva.CodigoReserva}";
                    return RedirectToAction("Detalles", new { codigo = resultado.Reserva.CodigoReserva });
                }
                else
                {
                    TempData["Error"] = resultado.Message;
                    return RedirectToAction("SeleccionarAsiento", new { viajeId });
                }
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

            var reserva = await _reservaService.ObtenerReservaPorCodigoAsync(codigo);

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

                var resultado = await _reservaService.CancelarReservaAsync(id, usuario.Id);

                if (resultado.Success)
                {
                    TempData["Success"] = resultado.Message;
                }
                else
                {
                    TempData["Error"] = resultado.Message;
                }
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

            var reserva = await _reservaService.ObtenerReservaPorCodigoAsync(codigo);
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
        public async Task<IActionResult> ProcesarPago(int reservaId, string metodoPago,
            string numeroTarjeta = "", string cvv = "", string mesExpiracion = "", string anioExpiracion = "",
            string nombreTarjeta = "", string cuentaOrigen = "", string banco = "")
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(reservaId);
                if (reserva == null)
                    return NotFound();

                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null || reserva.UsuarioId != usuario.Id)
                    return Forbid();

                // Preparar datos de pago según método
                var datosPago = new Dictionary<string, string>();

                if (metodoPago.ToLower() == "tarjeta")
                {
                    datosPago["numeroTarjeta"] = numeroTarjeta;
                    datosPago["cvv"] = cvv;
                    datosPago["mesExpiracion"] = mesExpiracion;
                    datosPago["anioExpiracion"] = anioExpiracion;
                    datosPago["nombreTarjeta"] = nombreTarjeta;
                }
                else if (metodoPago.ToLower() == "transferencia")
                {
                    datosPago["cuentaOrigen"] = cuentaOrigen;
                    datosPago["banco"] = banco;
                }

                var resultado = await _pagoService.ProcesarPagoAsync(reservaId, metodoPago, reserva.MontoTotal, datosPago);

                if (resultado.Success)
                {
                    TempData["Success"] = "Pago procesado exitosamente. Su reserva ha sido confirmada.";
                    TempData["ReferenciaPago"] = resultado.ReferenciaPago;
                    return RedirectToAction("Detalles", new { codigo = reserva.CodigoReserva });
                }
                else
                {
                    TempData["Error"] = resultado.Message;
                    return RedirectToAction("Pagar", new { id = reservaId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago");
                TempData["Error"] = "Error al procesar el pago. Intente nuevamente.";
                return RedirectToAction("Pagar", new { id = reservaId });
            }
        }

        public async Task<IActionResult> DescargarTicket(string codigo)
        {
            try
            {
                var reserva = await _reservaService.ObtenerReservaPorCodigoAsync(codigo);

                if (reserva == null)
                    return NotFound();

                var usuario = await _userManager.GetUserAsync(User);
                if (usuario == null || reserva.UsuarioId != usuario.Id)
                    return Forbid();

                var pdfBytes = await _pdfService.GenerarTicketReservaAsync(reserva);

                return File(pdfBytes, "application/pdf", $"Ticket_{codigo}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar ticket para reserva {codigo}");
                TempData["Error"] = "Error al generar el ticket";
                return RedirectToAction("Detalles", new { codigo });
            }
        }
    }
}