using Microsoft.AspNetCore.Identity;
using System;

namespace AgenciaDeTransporteWeb.Models.Entities
{
    public class Usuario : IdentityUser
    {
        public string NombreCompleto { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Documento { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }
}