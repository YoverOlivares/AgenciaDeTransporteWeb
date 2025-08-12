using System.ComponentModel.DataAnnotations;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class AuditoriaEvento
    {
        public int Id { get; set; }

        public string? UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        public string Accion { get; set; }

        [Required]
        [StringLength(50)]
        public string Entidad { get; set; }

        [Required]
        [StringLength(50)]
        public string EntidadId { get; set; }

        public string? ValoresAnteriores { get; set; }

        public string? ValoresNuevos { get; set; }

        public DateTime FechaEvento { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? DireccionIP { get; set; }

        public virtual Usuario? Usuario { get; set; }
    }
}