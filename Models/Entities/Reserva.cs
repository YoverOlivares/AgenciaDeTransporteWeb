using System;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Reserva
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; }
        public int ViajeId { get; set; }
        public int AsientoId { get; set; }
        public string CodigoReserva { get; set; }
        public DateTime FechaReserva { get; set; }
        public string EstadoReserva { get; set; }
        public decimal MontoTotal { get; set; }
        public Usuario Usuario { get; set; }
        public Viaje Viaje { get; set; }
        public Asiento Asiento { get; set; }
    }
}