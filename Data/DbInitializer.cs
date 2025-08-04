using Microsoft.AspNetCore.Identity;
using AgenciaDeTransporteWeb.Models.Entities;
using System.Threading.Tasks;

namespace AgenciaDeTransporteWeb.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Usuario" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@agenciadetransporte.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new Usuario
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador",
                    FechaRegistro = DateTime.Now,
                    Activo = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}