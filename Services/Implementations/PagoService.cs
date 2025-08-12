using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Services.Interfaces;
using AgenciaDeTransporteWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AgenciaDeTransporteWeb.Services.Implementations
{
    public class PagoService : IPagoService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PagoService> _logger;

        public PagoService(ApplicationDbContext context, ILogger<PagoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string ReferenciaPago)> ProcesarPagoAsync(
            int reservaId, string metodoPago, decimal monto, Dictionary<string, string> datosPago)
        {
            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Viaje)
                    .FirstOrDefaultAsync(r => r.Id == reservaId);

                if (reserva == null)
                    return (false, "Reserva no encontrada", string.Empty);

                if (reserva.EstadoReserva != "Pendiente")
                    return (false, "La reserva no está en estado pendiente", string.Empty);

                if (monto != reserva.MontoTotal)
                    return (false, "El monto no coincide con el total de la reserva", string.Empty);

                var referenciaPago = await GenerarReferenciaUnicaAsync();

                // Simular procesamiento según método de pago
                bool procesamientoExitoso = metodoPago.ToLower() switch
                {
                    "tarjeta" => await ProcesarPagoTarjetaAsync(datosPago, monto),
                    "efectivo" => await ProcesarPagoEfectivoAsync(monto),
                    "transferencia" => await ProcesarTransferenciaAsync(datosPago, monto),
                    _ => false
                };

                if (!procesamientoExitoso)
                    return (false, "Error al procesar el pago", string.Empty);

                // Crear transacción
                var transaccion = new Transaccion
                {
                    ReservaId = reservaId,
                    TipoTransaccion = "Pago",
                    MetodoPago = metodoPago,
                    Monto = monto,
                    FechaTransaccion = DateTime.Now,
                    EstadoTransaccion = "Completada",
                    ReferenciaPago = referenciaPago
                };

                _context.Transacciones.Add(transaccion);

                // Actualizar estado de reserva
                reserva.EstadoReserva = "Confirmada";

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Pago procesado exitosamente. Reserva: {reservaId}, Referencia: {referenciaPago}");

                return (true, "Pago procesado exitosamente", referenciaPago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar pago para reserva {reservaId}");
                return (false, "Error interno al procesar el pago", string.Empty);
            }
        }

        public async Task<(bool Success, string Message)> ValidarTarjetaAsync(string numeroTarjeta, string cvv,
            string mesExpiracion, string anioExpiracion)
        {
            await Task.Delay(100); // Simular tiempo de procesamiento

            // Validar formato de tarjeta
            if (string.IsNullOrEmpty(numeroTarjeta) || numeroTarjeta.Length < 13 || numeroTarjeta.Length > 19)
                return (false, "Número de tarjeta inválido");

            // Validar que solo contenga números
            if (!Regex.IsMatch(numeroTarjeta, @"^\d+$"))
                return (false, "El número de tarjeta debe contener solo dígitos");

            // Validar CVV
            if (string.IsNullOrEmpty(cvv) || cvv.Length < 3 || cvv.Length > 4 || !Regex.IsMatch(cvv, @"^\d+$"))
                return (false, "CVV inválido");

            // Validar fecha de expiración
            if (!int.TryParse(mesExpiracion, out int mes) || mes < 1 || mes > 12)
                return (false, "Mes de expiración inválido");

            if (!int.TryParse(anioExpiracion, out int anio) || anio < DateTime.Now.Year)
                return (false, "Año de expiración inválido");

            // Validar que no esté expirada
            var fechaExpiracion = new DateTime(anio, mes, DateTime.DaysInMonth(anio, mes));
            if (fechaExpiracion < DateTime.Now)
                return (false, "La tarjeta está expirada");

            // Algoritmo de Luhn para validar número de tarjeta
            if (!ValidarAlgoritmoLuhn(numeroTarjeta))
                return (false, "Número de tarjeta inválido");

            return (true, "Tarjeta válida");
        }

        public async Task<bool> VerificarFondosAsync(string numeroTarjeta, decimal monto)
        {
            await Task.Delay(200); // Simular tiempo de procesamiento

            // Simulación: algunas tarjetas específicas no tienen fondos
            var tarjetasSinFondos = new[] { "4111111111111111", "4000000000000002" };

            if (tarjetasSinFondos.Contains(numeroTarjeta))
                return false;

            // Simulación: montos muy altos no tienen fondos
            if (monto > 10000)
                return false;

            return true; // Simular que tiene fondos
        }

        public async Task<(bool Success, string Message)> ProcesarReembolsoAsync(int transaccionId, decimal monto)
        {
            try
            {
                var transaccion = await _context.Transacciones
                    .Include(t => t.Reserva)
                    .FirstOrDefaultAsync(t => t.Id == transaccionId);

                if (transaccion == null)
                    return (false, "Transacción no encontrada");

                if (transaccion.EstadoTransaccion != "Completada")
                    return (false, "La transacción no está completada");

                if (monto > transaccion.Monto)
                    return (false, "El monto del reembolso no puede ser mayor al monto pagado");

                // Crear transacción de reembolso
                var reembolso = new Transaccion
                {
                    ReservaId = transaccion.ReservaId,
                    TipoTransaccion = "Reembolso",
                    MetodoPago = transaccion.MetodoPago,
                    Monto = -monto, // Negativo para indicar reembolso
                    FechaTransaccion = DateTime.Now,
                    EstadoTransaccion = "Completada",
                    ReferenciaPago = await GenerarReferenciaUnicaAsync()
                };

                _context.Transacciones.Add(reembolso);

                // Actualizar estado de reserva si es reembolso total
                if (monto == transaccion.Monto)
                {
                    transaccion.Reserva.EstadoReserva = "Cancelada";
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Reembolso procesado. Transacción: {transaccionId}, Monto: {monto}");

                return (true, "Reembolso procesado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar reembolso para transacción {transaccionId}");
                return (false, "Error interno al procesar el reembolso");
            }
        }

        public async Task<string> GenerarReferenciaUnicaAsync()
        {
            await Task.Delay(10);

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var random = new Random().Next(1000, 9999);

            return $"PAY{timestamp}{random}";
        }

        public async Task<bool> ConfirmarPagoAsync(string referenciaPago)
        {
            try
            {
                var transaccion = await _context.Transacciones
                    .FirstOrDefaultAsync(t => t.ReferenciaPago == referenciaPago);

                if (transaccion == null)
                    return false;

                // Simulación de confirmación externa
                await Task.Delay(100);

                return transaccion.EstadoTransaccion == "Completada";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al confirmar pago con referencia {referenciaPago}");
                return false;
            }
        }

        private async Task<bool> ProcesarPagoTarjetaAsync(Dictionary<string, string> datosPago, decimal monto)
        {
            if (!datosPago.ContainsKey("numeroTarjeta") || !datosPago.ContainsKey("cvv") ||
                !datosPago.ContainsKey("mesExpiracion") || !datosPago.ContainsKey("anioExpiracion"))
                return false;

            var validacion = await ValidarTarjetaAsync(
                datosPago["numeroTarjeta"],
                datosPago["cvv"],
                datosPago["mesExpiracion"],
                datosPago["anioExpiracion"]);

            if (!validacion.Success)
                return false;

            var tieneFondos = await VerificarFondosAsync(datosPago["numeroTarjeta"], monto);
            if (!tieneFondos)
                return false;

            // Simular tiempo de procesamiento
            await Task.Delay(500);

            return true;
        }

        private async Task<bool> ProcesarPagoEfectivoAsync(decimal monto)
        {
            // Simulación: el efectivo siempre se procesa exitosamente
            await Task.Delay(100);
            return true;
        }

        private async Task<bool> ProcesarTransferenciaAsync(Dictionary<string, string> datosPago, decimal monto)
        {
            if (!datosPago.ContainsKey("cuentaOrigen") || !datosPago.ContainsKey("banco"))
                return false;

            // Simular validación de cuenta bancaria
            await Task.Delay(300);

            var cuentaOrigen = datosPago["cuentaOrigen"];

            // Simulación: cuentas que empiecen con "000" fallan
            if (cuentaOrigen.StartsWith("000"))
                return false;

            return true;
        }

        // CORREGIDO: Agregar static para resolver CA1822
        private static bool ValidarAlgoritmoLuhn(string numeroTarjeta)
        {
            int suma = 0;
            bool esSegundo = false;

            for (int i = numeroTarjeta.Length - 1; i >= 0; i--)
            {
                int digito = numeroTarjeta[i] - '0';

                if (esSegundo)
                {
                    digito *= 2;
                    if (digito > 9)
                        digito = digito / 10 + digito % 10;
                }

                suma += digito;
                esSegundo = !esSegundo;
            }

            return suma % 10 == 0;
        }
    }
}