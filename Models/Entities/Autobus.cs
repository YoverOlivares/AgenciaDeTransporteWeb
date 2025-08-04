using System;
using System.Collections.Generic;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Autobus
    {
        public int Id { get; set; }
        public string Placa { get; set; }
        public string Modelo { get; set; }
        public string Marca { get; set; }
        public int CapacidadAsientos { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public List<Asiento> Asientos { get; set; } = new List<Asiento>();
    }
}