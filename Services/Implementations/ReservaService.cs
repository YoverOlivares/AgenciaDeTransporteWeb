using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Models.DTOs;
using AgenciaDeTransporteWeb.Services.Interfaces;
using AgenciaDeTransporteWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace AgenciaDeTransporteWeb.Services.Implementations
{
    public class ReservaService : IReservaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReservaService> _logger;

        public ReservaService(ApplicationDbContext context, ILogger<ReservaService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, Reserva? Reserva)> CrearReservaAsync(
            string usuarioId, int viajeId, int asientoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Verificar que el usuario existe
                var usuario = await _context.Users.FindAsync(usuarioId);
                if (usuario == null)
                    return (false, "Usuario no encontrado", null);

                // Verificar que el viaje existe y está disponible
                var viaje = await _context.Viajes
                    .Include(v => v.Ruta)
                    .FirstOrDefaultAsync(v => v.Id == viajeId);

                if (viaje == null)
                    return (false, "Viaje no encontrado", null);

                if (viaje.Estado != "Programado")
                    return (false, "El viaje no está disponible para reservas", null);

                if (viaje.FechaSalida <= DateTime.Now.AddHours(2))
                    return (false, "No se pueden hacer reservas con menos de 2 horas de anticipación", null);

                // Verificar disponibilidad del asiento
                var asientoDisponible = await VerificarDisponibilidadAsientoAsync(viajeId, asientoId);
                if (!asientoDisponible)
                    return (false, "El asiento seleccionado no está disponible", null);

                // Obtener información del asiento
                var asiento = await _context.Asientos
                    .FirstOrDefaultAsync(a => a.Id == asientoId && a.Activo);

                if (asiento == null)
                    return (false, "Asiento no encontrado", null);

                // Verificar que el asiento pertenece al autobús del viaje
                if (asiento.AutobusId != viaje.AutobusId)
                    return (false, "El asiento no pertenece al autobús de este viaje", null);

                // Generar código único de reserva
                var codigoReserva = await GenerarCodigoReservaUnicoAsync();

                // Calcular precio total
                var precioTotal = await CalcularPrecioTotalAsync(viajeId, asientoId);

                // Crear reserva
                var reserva = new Reserva
                {
                    UsuarioId = usuarioId,
                    ViajeId = viajeId,
                    AsientoId = asientoId,
                    CodigoReserva = codigoReserva,
                    FechaReserva = DateTime.Now,
                    EstadoReserva = "Pendiente",
                    MontoTotal = precioTotal
                };

                _context.Reservas.Add(reserva);

                // Reducir asientos disponibles
                viaje.AsientosDisponibles--;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Reserva creada exitosamente. Código: {codigoReserva}, Usuario: {usuarioId}");

                return (true, "Reserva creada exitosamente", reserva);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al crear reserva para usuario {usuarioId}, viaje {viajeId}");
                return (false, "Error interno al crear la reserva", null);
            }
        }

        public async Task<(bool Success, string Message)> CancelarReservaAsync(int reservaId, string usuarioId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Viaje)
                    .FirstOrDefaultAsync(r => r.Id == reservaId && r.UsuarioId == usuarioId);

                if (reserva == null)
                    return (false, "Reserva no encontrada o no pertenece al usuario");

                if (reserva.EstadoReserva == "Cancelada")
                    return (false, "La reserva ya está cancelada");

                if (reserva.EstadoReserva == "Completada")
                    return (false, "No se puede cancelar una reserva completada");

                // Verificar tiempo límite para cancelación (2 horas antes)
                if (reserva.Viaje.FechaSalida <= DateTime.Now.AddHours(2))
                    return (false, "No se puede cancelar con menos de 2 horas de anticipación");

                // Cancelar reserva
                reserva.EstadoReserva = "Cancelada";

                // Liberar asiento
                reserva.Viaje.AsientosDisponibles++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Reserva cancelada exitosamente. ID: {reservaId}, Usuario: {usuarioId}");

                return (true, "Reserva cancelada exitosamente");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error al cancelar reserva {reservaId} para usuario {usuarioId}");
                return (false, "Error interno al cancelar la reserva");
            }
        }

        public async Task<bool> VerificarDisponibilidadAsientoAsync(int viajeId, int asientoId)
        {
            try
            {
                var reservaExistente = await _context.Reservas
                    .AnyAsync(r => r.ViajeId == viajeId &&
                                  r.AsientoId == asientoId &&
                                  r.EstadoReserva != "Cancelada");

                return !reservaExistente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al verificar disponibilidad del asiento {asientoId} para viaje {viajeId}");
                return false;
            }
        }

        public async Task<List<Asiento>> ObtenerAsientosDisponiblesAsync(int viajeId)
        {
            try
            {
                var viaje = await _context.Viajes
                    .Include(v => v.Autobus)
                        .ThenInclude(a => a.Asientos)
                    .FirstOrDefaultAsync(v => v.Id == viajeId);

                if (viaje == null)
                    return new List<Asiento>();

                // Obtener asientos ocupados
                var asientosOcupados = await _context.Reservas
                    .Where(r => r.ViajeId == viajeId && r.EstadoReserva != "Cancelada")
                    .Select(r => r.AsientoId)
                    .ToListAsync();

                // Filtrar asientos disponibles
                var asientosDisponibles = viaje.Autobus.Asientos
                    .Where(a => a.Activo && !asientosOcupados.Contains(a.Id))
                    .OrderBy(a => a.NumeroAsiento)
                    .ToList();

                return asientosDisponibles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener asientos disponibles para viaje {viajeId}");
                return new List<Asiento>();
            }
        }

        public async Task<Reserva?> ObtenerReservaPorCodigoAsync(string codigoReserva)
        {
            try
            {
                return await _context.Reservas
                    .Include(r => r.Usuario)
                    .Include(r => r.Viaje)
                        .ThenInclude(v => v.Ruta)
                    .Include(r => r.Viaje)
                        .ThenInclude(v => v.Autobus)
                    .Include(r => r.Asiento)
                    .FirstOrDefaultAsync(r => r.CodigoReserva == codigoReserva);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reserva con código {codigoReserva}");
                return null;
            }
        }

        public async Task<List<Reserva>> ObtenerReservasUsuarioAsync(string usuarioId)
        {
            try
            {
                return await _context.Reservas
                    .Include(r => r.Viaje)
                        .ThenInclude(v => v.Ruta)
                    .Include(r => r.Viaje)
                        .ThenInclude(v => v.Autobus)
                    .Include(r => r.Asiento)
                    .Where(r => r.UsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaReserva)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener reservas del usuario {usuarioId}");
                return new List<Reserva>();
            }
        }

        public async Task<(bool Success, string Message)> ConfirmarReservaAsync(int reservaId)
        {
            try
            {
                var reserva = await _context.Reservas.FindAsync(reservaId);

                if (reserva == null)
                    return (false, "Reserva no encontrada");

                if (reserva.EstadoReserva != "Pendiente")
                    return (false, "La reserva no está en estado pendiente");

                reserva.EstadoReserva = "Confirmada";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Reserva confirmada exitosamente. ID: {reservaId}");

                return (true, "Reserva confirmada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al confirmar reserva {reservaId}");
                return (false, "Error interno al confirmar la reserva");
            }
        }

        public async Task<string> GenerarCodigoReservaUnicoAsync()
        {
            string codigo;
            bool existe;

            do
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd");
                var random = new Random().Next(1000, 9999);
                codigo = $"RES{timestamp}{random}";

                existe = await _context.Reservas
                    .AnyAsync(r => r.CodigoReserva == codigo);

            } while (existe);

            return codigo;
        }

        public async Task<decimal> CalcularPrecioTotalAsync(int viajeId, int asientoId)
        {
            try
            {
                var viaje = await _context.Viajes
                    .Include(v => v.Ruta)
                    .FirstOrDefaultAsync(v => v.Id == viajeId);

                var asiento = await _context.Asientos
                    .FirstOrDefaultAsync(a => a.Id == asientoId);

                if (viaje == null || asiento == null)
                    return 0;

                decimal precioBase = viaje.PrecioViaje;

                // Aplicar recargo según tipo de asiento
                decimal recargo = asiento.TipoAsiento.ToLower() switch
                {
                    "vip" => precioBase * 0.25m,        // 25% más caro
                    "cama" => precioBase * 0.50m,       // 50% más caro
                    _ => 0                               // Estándar sin recargo
                };

                return precioBase + recargo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al calcular precio total para viaje {viajeId}, asiento {asientoId}");
                return 0;
            }
        }

        public async Task<bool> ValidarTiempoLimiteReservaAsync(int reservaId)
        {
            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Viaje)
                    .FirstOrDefaultAsync(r => r.Id == reservaId);

                if (reserva == null)
                    return false;

                // La reserva es válida si faltan más de 30 minutos para el viaje
                // y no han pasado más de 24 horas desde que se hizo la reserva
                var tiempoLimiteViaje = reserva.Viaje.FechaSalida.AddMinutes(-30);
                var tiempoLimiteReserva = reserva.FechaReserva.AddHours(24);

                return DateTime.Now < tiempoLimiteViaje && DateTime.Now < tiempoLimiteReserva;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar tiempo límite de reserva {reservaId}");
                return false;
            }
        }

        public async Task LiberarReservasExpiradas()
        {
            try
            {
                var reservasExpiradas = await _context.Reservas
                    .Include(r => r.Viaje)
                    .Where(r => r.EstadoReserva == "Pendiente" &&
                               (r.FechaReserva.AddHours(24) < DateTime.Now ||
                                r.Viaje.FechaSalida.AddMinutes(-30) < DateTime.Now))
                    .ToListAsync();

                // CORREGIDO: Usar Count en lugar de Any() para resolver CA1860
                if (reservasExpiradas.Count > 0)
                {
                    foreach (var reserva in reservasExpiradas)
                    {
                        reserva.EstadoReserva = "Cancelada";
                        reserva.Viaje.AsientosDisponibles++;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Se liberaron {reservasExpiradas.Count} reservas expiradas");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al liberar reservas expiradas");
            }
        }
    }
}