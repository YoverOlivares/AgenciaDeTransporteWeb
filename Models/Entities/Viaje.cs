using System;

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
        public string Estado { get; set; }
        public int AsientosDisponibles { get; set; }
        public Ruta Ruta { get; set; }
        public Autobus Autobus { get; set; }
    }
}