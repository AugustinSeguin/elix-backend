using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElixBackend.WebApp.Controllers;

[Authorize]
[Route("[controller]")]
public class CategoryController : Controller
{
    [HttpGet("[action]")] 
    public IActionResult Index()
    {
        return View();
    }

}