using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgenciaDeTransporteWeb.Data;
using System.Threading.Tasks;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class RutasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RutasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var rutas = await _context.Rutas
                .Where(r => r.Activo)
                .ToListAsync();
            return View(rutas);
        }
    }
}