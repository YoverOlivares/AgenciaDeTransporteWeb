using System;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class AuditoriaEvento
    {
        public int Id { get; set; }
        public string? UsuarioId { get; set; }
        public string Accion { get; set; }
        public string Entidad { get; set; }
        public string EntidadId { get; set; }
        public string? ValoresAnteriores { get; set; }
        public string? ValoresNuevos { get; set; }
        public DateTime FechaEvento { get; set; }
        public string? DireccionIP { get; set; }
        public Usuario? Usuario { get; set; }
    }
}