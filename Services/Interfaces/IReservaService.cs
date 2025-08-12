using AgenciaDeTransporteWeb.Models.Entities;
using AgenciaDeTransporteWeb.Models.DTOs;

namespace AgenciaDeTransporteWeb.Services.Interfaces
{
    public interface IReservaService
    {
        Task<(bool Success, string Message, Reserva? Reserva)> CrearReservaAsync(
            string usuarioId, int viajeId, int asientoId);

        Task<(bool Success, string Message)> CancelarReservaAsync(int reservaId, string usuarioId);

        Task<bool> VerificarDisponibilidadAsientoAsync(int viajeId, int asientoId);

        Task<List<Asiento>> ObtenerAsientosDisponiblesAsync(int viajeId);

        Task<Reserva?> ObtenerReservaPorCodigoAsync(string codigoReserva);

        Task<List<Reserva>> ObtenerReservasUsuarioAsync(string usuarioId);

        Task<(bool Success, string Message)> ConfirmarReservaAsync(int reservaId);

        Task<string> GenerarCodigoReservaUnicoAsync();

        Task<decimal> CalcularPrecioTotalAsync(int viajeId, int asientoId);

        Task<bool> ValidarTiempoLimiteReservaAsync(int reservaId);

        Task LiberarReservasExpiradas();
    }
}