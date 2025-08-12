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

            // Configuración de Asiento
            builder.Entity<Asiento>()
                .HasOne<Autobus>(a => a.Autobus)
                .WithMany(b => b.Asientos)
                .HasForeignKey(a => a.AutobusId);

            builder.Entity<Asiento>()
                .HasIndex(a => new { a.AutobusId, a.NumeroAsiento })
                .IsUnique();

            // Configuración de Viaje
            builder.Entity<Viaje>()
                .HasOne<Ruta>(v => v.Ruta)
                .WithMany()
                .HasForeignKey(v => v.RutaId);

            builder.Entity<Viaje>()
                .HasOne<Autobus>(v => v.Autobus)
                .WithMany()
                .HasForeignKey(v => v.AutobusId);

            // Configuración de Reserva
            builder.Entity<Reserva>()
                .HasOne<Usuario>(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId);

            builder.Entity<Reserva>()
                .HasOne<Viaje>(r => r.Viaje)
                .WithMany()
                .HasForeignKey(r => r.ViajeId);

            builder.Entity<Reserva>()
                .HasOne<Asiento>(r => r.Asiento)
                .WithMany()
                .HasForeignKey(r => r.AsientoId);

            builder.Entity<Reserva>()
                .HasIndex(r => r.CodigoReserva)
                .IsUnique();

            // Configuración de Transaccion
            builder.Entity<Transaccion>()
                .HasOne<Reserva>(t => t.Reserva)
                .WithMany()
                .HasForeignKey(t => t.ReservaId);

            // Configuración de AuditoriaEvento
            builder.Entity<AuditoriaEvento>()
                .HasOne<Usuario>(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de precisión decimal
            builder.Entity<Ruta>()
                .Property(r => r.Precio)
                .HasPrecision(10, 2);

            builder.Entity<Ruta>()
                .Property(r => r.DuracionHoras)
                .HasPrecision(4, 2);

            builder.Entity<Ruta>()
                .Property(r => r.DistanciaKm)
                .HasPrecision(8, 2);

            builder.Entity<Viaje>()
                .Property(v => v.PrecioViaje)
                .HasPrecision(10, 2);

            builder.Entity<Reserva>()
                .Property(r => r.MontoTotal)
                .HasPrecision(10, 2);

            builder.Entity<Transaccion>()
                .Property(t => t.Monto)
                .HasPrecision(10, 2);
        }
    }
}