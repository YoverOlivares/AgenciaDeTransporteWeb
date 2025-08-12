using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Reserva
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; }

        public int ViajeId { get; set; }

        public int AsientoId { get; set; }

        [StringLength(20)]
        public string CodigoReserva { get; set; }

        public DateTime FechaReserva { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string EstadoReserva { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada, Completada

        public decimal MontoTotal { get; set; }

        public virtual Usuario Usuario { get; set; }
        public virtual Viaje Viaje { get; set; }
        public virtual Asiento Asiento { get; set; }
    }
}