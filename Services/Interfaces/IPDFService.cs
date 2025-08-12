using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Services.Interfaces
{
    public interface IPDFService
    {
        Task<byte[]> GenerarTicketReservaAsync(Reserva reserva);
        Task<byte[]> GenerarReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> GenerarReporteRutasAsync();
        Task<byte[]> GenerarManifiestoViajeAsync(int viajeId);
        Task<byte[]> GenerarFacturaAsync(Transaccion transaccion);
    }
}