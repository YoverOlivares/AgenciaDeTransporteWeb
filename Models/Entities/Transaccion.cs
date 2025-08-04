using System;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int ReservaId { get; set; }
        public string TipoTransaccion { get; set; }
        public string MetodoPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string EstadoTransaccion { get; set; }
        public string? ReferenciaPago { get; set; }
        public Reserva Reserva { get; set; }
    }
}