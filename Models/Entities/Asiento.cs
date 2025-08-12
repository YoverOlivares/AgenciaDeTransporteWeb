using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Asiento
    {
        public int Id { get; set; }

        public int AutobusId { get; set; }

        public int NumeroAsiento { get; set; }

        [StringLength(20)]
        public string TipoAsiento { get; set; } = "Estándar"; // Estándar, VIP, Cama

        public bool Activo { get; set; } = true;

        public virtual Autobus Autobus { get; set; }
    }
}