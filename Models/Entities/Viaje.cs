using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Viaje
    {
        public int Id { get; set; }

        public int RutaId { get; set; }

        public int AutobusId { get; set; }

        public DateTime FechaSalida { get; set; }

        public DateTime FechaLlegada { get; set; }

        public decimal PrecioViaje { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Programado"; // Programado, En Ruta, Completado, Cancelado

        public int AsientosDisponibles { get; set; }

        public virtual Ruta Ruta { get; set; }
        public virtual Autobus Autobus { get; set; }
    }
}