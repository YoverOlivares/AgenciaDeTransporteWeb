using Microsoft.AspNetCore.Mvc;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class ReservasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
