using System.ComponentModel.DataAnnotations;
using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Models.ViewModels
{
    public class ReservaViewModel
    {
        public int ViajeId { get; set; }
        public int AsientoId { get; set; }
        public decimal MontoTotal { get; set; }

        [Display(Name = "Código de Reserva")]
        public string CodigoReserva { get; set; }

        [Display(Name = "Estado")]
        public string EstadoReserva { get; set; }

        [Display(Name = "Fecha de Reserva")]
        public DateTime FechaReserva { get; set; }

        // Información del viaje
        [Display(Name = "Origen")]
        public string CiudadOrigen { get; set; }

        [Display(Name = "Destino")]
        public string CiudadDestino { get; set; }

        [Display(Name = "Fecha de Salida")]
        public DateTime FechaSalida { get; set; }

        [Display(Name = "Fecha de Llegada")]
        public DateTime FechaLlegada { get; set; }

        [Display(Name = "Número de Asiento")]
        public int NumeroAsiento { get; set; }

        [Display(Name = "Tipo de Asiento")]
        public string TipoAsiento { get; set; }

        [Display(Name = "Placa del Autobús")]
        public string PlacaAutobus { get; set; }

        // Para creación de reserva
        public Viaje Viaje { get; set; }
        public List<Asiento> AsientosDisponibles { get; set; } = new List<Asiento>();
    }
}