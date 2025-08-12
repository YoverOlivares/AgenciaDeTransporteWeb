using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Models.ViewModels
{
    public class SeleccionAsientoViewModel
    {
        public int ViajeId { get; set; }
        public Viaje Viaje { get; set; }
        public List<Asiento> AsientosDisponibles { get; set; } = new List<Asiento>();
        public List<int> AsientosOcupados { get; set; } = new List<int>();

        public int? AsientoSeleccionadoId { get; set; }
        public decimal PrecioTotal { get; set; }

        // Información del autobús para mostrar el layout
        public int CapacidadAsientos { get; set; }
        public string PlacaAutobus { get; set; }
        public string ModeloAutobus { get; set; }
    }
}