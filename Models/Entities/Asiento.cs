namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Asiento
    {
        public int Id { get; set; }
        public int AutobusId { get; set; }
        public int NumeroAsiento { get; set; }
        public string TipoAsiento { get; set; }
        public bool Activo { get; set; }
        public Autobus Autobus { get; set; }
    }
}