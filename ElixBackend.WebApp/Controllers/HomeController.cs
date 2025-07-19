using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElixBackend.WebApp.Controllers;

[Route("[controller]")]
public class HomeController : Controller
{
    [HttpGet("index")]
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

}