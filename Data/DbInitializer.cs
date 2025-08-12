// Data/DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Models.Entities;

namespace AgenciaDeTransporteWeb.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Crear roles si no existen
            await CrearRoles(roleManager);

            // Crear usuario administrador por defecto
            await CrearUsuarioAdmin(userManager);
        }

        private static async Task CrearRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Cliente" };

            foreach (var roleName in roleNames)
            {
                var roleExiste = await roleManager.RoleExistsAsync(roleName);
                if (!roleExiste)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task CrearUsuarioAdmin(UserManager<Usuario> userManager)
        {
            var adminEmail = "admin@agencia.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true,
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };

                var resultado = await userManager.CreateAsync(adminUser, "Admin123!");

                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Crear rutas de ejemplo
            if (!context.Rutas.Any())
            {
                var rutas = new List<Ruta>
                {
                    new Ruta
                    {
                        CiudadOrigen = "Lima",
                        CiudadDestino = "Arequipa",
                        Precio = 45.00m,
                        DuracionHoras = 16.5m,
                        DistanciaKm = 1009m,
                        Activo = true
                    },
                    new Ruta
                    {
                        CiudadOrigen = "Lima",
                        CiudadDestino = "Cusco",
                        Precio = 55.00m,
                        DuracionHoras = 20.0m,
                        DistanciaKm = 1153m,
                        Activo = true
                    },
                    new Ruta
                    {
                        CiudadOrigen = "Lima",
                        CiudadDestino = "Trujillo",
                        Precio = 35.00m,
                        DuracionHoras = 8.5m,
                        DistanciaKm = 561m,
                        Activo = true
                    },
                    new Ruta
                    {
                        CiudadOrigen = "Arequipa",
                        CiudadDestino = "Cusco",
                        Precio = 25.00m,
                        DuracionHoras = 10.0m,
                        DistanciaKm = 479m,
                        Activo = true
                    }
                };

                context.Rutas.AddRange(rutas);
                await context.SaveChangesAsync();
            }

            // Crear autobuses de ejemplo
            if (!context.Autobuses.Any())
            {
                var autobuses = new List<Autobus>
                {
                    new Autobus
                    {
                        Placa = "ABC-123",
                        Modelo = "Volvo 9700",
                        Marca = "Volvo",
                        CapacidadAsientos = 42,
                        Activo = true
                    },
                    new Autobus
                    {
                        Placa = "DEF-456",
                        Modelo = "Mercedes O500",
                        Marca = "Mercedes-Benz",
                        CapacidadAsientos = 38,
                        Activo = true
                    }
                };

                context.Autobuses.AddRange(autobuses);
                await context.SaveChangesAsync();

                // Crear asientos para cada autobús
                foreach (var autobus in autobuses)
                {
                    var asientos = new List<Asiento>();

                    for (int i = 1; i <= autobus.CapacidadAsientos; i++)
                    {
                        string tipoAsiento = "Estándar";

                        // Asientos VIP (primeras 2 filas)
                        if (i <= 8)
                            tipoAsiento = "VIP";
                        // Asientos cama (últimas 2 filas)
                        else if (i > autobus.CapacidadAsientos - 8)
                            tipoAsiento = "Cama";

                        asientos.Add(new Asiento
                        {
                            AutobusId = autobus.Id,
                            NumeroAsiento = i,
                            TipoAsiento = tipoAsiento,
                            Activo = true
                        });
                    }

                    context.Asientos.AddRange(asientos);
                }
                await context.SaveChangesAsync();
            }

            // Crear viajes de ejemplo
            if (!context.Viajes.Any())
            {
                var rutas = await context.Rutas.ToListAsync();
                var autobuses = await context.Autobuses.ToListAsync();

                var viajes = new List<Viaje>();
                var fechaBase = DateTime.Now.Date.AddDays(1); // Empezar mañana

                for (int dia = 0; dia < 14; dia++) // 2 semanas de viajes
                {
                    foreach (var ruta in rutas)
                    {
                        var autobus = autobuses[Random.Shared.Next(autobuses.Count)];
                        var horasSalida = new[] { 6, 14, 22 }; // 6am, 2pm, 10pm

                        foreach (var hora in horasSalida)
                        {
                            var fechaSalida = fechaBase.AddDays(dia).AddHours(hora);
                            var fechaLlegada = fechaSalida.AddHours((double)ruta.DuracionHoras);

                            viajes.Add(new Viaje
                            {
                                RutaId = ruta.Id,
                                AutobusId = autobus.Id,
                                FechaSalida = fechaSalida,
                                FechaLlegada = fechaLlegada,
                                PrecioViaje = ruta.Precio,
                                Estado = "Programado",
                                AsientosDisponibles = autobus.CapacidadAsientos
                            });
                        }
                    }
                }

                context.Viajes.AddRange(viajes);
                await context.SaveChangesAsync();
            }
        }
    }
}