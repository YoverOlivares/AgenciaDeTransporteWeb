using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Ruta
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CiudadOrigen { get; set; }

        [Required]
        [StringLength(100)]
        public string CiudadDestino { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public decimal DuracionHoras { get; set; }

        public decimal DistanciaKm { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}