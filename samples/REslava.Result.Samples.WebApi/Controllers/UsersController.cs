using Microsoft.AspNetCore.Mvc;

namespace REslava.Result.Samples.WebApi.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
