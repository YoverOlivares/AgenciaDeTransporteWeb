namespace AgenciaDeTransporteWeb.Models.DTOs
{
    public class RutaDto
    {
        public int Id { get; set; }
        public string CiudadOrigen { get; set; }
        public string CiudadDestino { get; set; }
        public decimal Precio { get; set; }
        public decimal DuracionHoras { get; set; }
        public decimal DistanciaKm { get; set; }
        public bool Activo { get; set; }
    }
}