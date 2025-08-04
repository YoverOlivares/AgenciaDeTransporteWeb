using System;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Ruta
    {
        public int Id { get; set; }
        public string CiudadOrigen { get; set; }
        public string CiudadDestino { get; set; }
        public decimal Precio { get; set; }
        public decimal DuracionHoras { get; set; }
        public decimal DistanciaKm { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}