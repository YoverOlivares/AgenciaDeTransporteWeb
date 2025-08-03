using Microsoft.AspNetCore.Mvc;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
