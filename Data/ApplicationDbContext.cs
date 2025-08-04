using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Autobus> Autobuses { get; set; }
        public DbSet<Ruta> Rutas { get; set; }
        public DbSet<Asiento> Asientos { get; set; }
        public DbSet<Viaje> Viajes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<AuditoriaEvento> AuditoriaEventos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Asiento>()
                .HasOne(a => a.Autobus)
                .WithMany(b => b.Asientos)
                .HasForeignKey(a => a.AutobusId);

            builder.Entity<Viaje>()
                .HasOne(v => v.Ruta)
                .WithMany()
                .HasForeignKey(v => v.RutaId);

            builder.Entity<Viaje>()
                .HasOne(v => v.Autobus)
                .WithMany()
                .HasForeignKey(v => v.AutobusId);

            builder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId);

            builder.Entity<Reserva>()
                .HasOne(r => r.Viaje)
                .WithMany()
                .HasForeignKey(r => r.ViajeId);

            builder.Entity<Reserva>()
                .HasOne(r => r.Asiento)
                .WithMany()
                .HasForeignKey(r => r.AsientoId);

            builder.Entity<Transaccion>()
                .HasOne(t => t.Reserva)
                .WithMany()
                .HasForeignKey(t => t.ReservaId);

            builder.Entity<AuditoriaEvento>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}