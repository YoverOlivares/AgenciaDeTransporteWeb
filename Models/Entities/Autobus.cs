using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Autobus
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Placa { get; set; }

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; }

        [Required]
        [StringLength(50)]
        public string Marca { get; set; }

        public int CapacidadAsientos { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public virtual ICollection<Asiento> Asientos { get; set; } = new List<Asiento>();
    }
}