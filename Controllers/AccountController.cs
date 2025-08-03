using Microsoft.AspNetCore.Mvc;

namespace AgenciaDeTransporteWeb.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
