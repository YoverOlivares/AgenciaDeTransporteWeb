namespace AgenciaDeTransporteWeb.Models.DTOs
{
    public class ReservaDto
    {
        public int Id { get; set; }
        public string CodigoReserva { get; set; }
        public DateTime FechaReserva { get; set; }
        public string EstadoReserva { get; set; }
        public decimal MontoTotal { get; set; }

        // Información del viaje
        public string CiudadOrigen { get; set; }
        public string CiudadDestino { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }

        // Información del asiento
        public int NumeroAsiento { get; set; }
        public string TipoAsiento { get; set; }

        // Información del usuario
        public string NombreUsuario { get; set; }
        public string EmailUsuario { get; set; }
    }
}