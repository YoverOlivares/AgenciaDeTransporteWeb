using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Transaccion
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }

        [StringLength(20)]
        public string TipoTransaccion { get; set; } // Pago, Reembolso

        [StringLength(50)]
        public string MetodoPago { get; set; } // Tarjeta, Efectivo, Transferencia

        public decimal Monto { get; set; }

        public DateTime FechaTransaccion { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string EstadoTransaccion { get; set; } = "Pendiente"; // Pendiente, Completada, Fallida

        [StringLength(100)]
        public string? ReferenciaPago { get; set; }

        public virtual Reserva Reserva { get; set; }
    }
}