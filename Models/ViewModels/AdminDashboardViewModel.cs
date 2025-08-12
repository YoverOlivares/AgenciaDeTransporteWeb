using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Estadísticas generales
        public int TotalUsuarios { get; set; }
        public int TotalReservas { get; set; }
        public int TotalAutobuses { get; set; }
        public int TotalRutas { get; set; }

        public decimal IngresosMensuales { get; set; }
        public decimal IngresosAnuales { get; set; }

        // Reservas recientes
        public List<Reserva> ReservasRecientes { get; set; } = new List<Reserva>();

        // Viajes programados próximos
        public List<Viaje> ViajesProximos { get; set; } = new List<Viaje>();

        // Transacciones recientes
        public List<Transaccion> TransaccionesRecientes { get; set; } = new List<Transaccion>();

        // Estadísticas por mes (para gráficos)
        public Dictionary<string, decimal> IngresosPorMes { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> ReservasPorMes { get; set; } = new Dictionary<string, int>();

        // Rutas más populares
        public List<dynamic> RutasPopulares { get; set; } = new List<dynamic>();
    }
}