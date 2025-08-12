using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Services.Interfaces
{
    public interface IPagoService
    {
        Task<(bool Success, string Message, string ReferenciaPago)> ProcesarPagoAsync(
            int reservaId, string metodoPago, decimal monto, Dictionary<string, string> datosPago);

        Task<(bool Success, string Message)> ValidarTarjetaAsync(string numeroTarjeta, string cvv,
            string mesExpiracion, string anioExpiracion);

        Task<bool> VerificarFondosAsync(string numeroTarjeta, decimal monto);

        Task<(bool Success, string Message)> ProcesarReembolsoAsync(int transaccionId, decimal monto);

        Task<string> GenerarReferenciaUnicaAsync();

        Task<bool> ConfirmarPagoAsync(string referenciaPago);
    }
}