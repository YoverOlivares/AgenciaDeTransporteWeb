using Microsoft.AspNetCore.Mvc;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class RutasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
