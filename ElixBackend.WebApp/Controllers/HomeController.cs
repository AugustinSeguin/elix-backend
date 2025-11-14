using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;

namespace ElixBackend.WebApp.Controllers;
    
[Authorize]
[Route("[controller]")]
public class HomeController(IUserService userService) : Controller
{
    [HttpGet("[action]")] 
    public async Task<IActionResult> Index()
    {
        var email = User?.Identity?.Name;
        if (!string.IsNullOrEmpty(email))
        {
            var user = await userService.GetUserByEmailAsync(email);
            ViewBag.UserFirstname = user?.Firstname ?? "Utilisateur";
        }
        else
        {
            ViewBag.UserFirstname = "Utilisateur";
        }
        
        return View();
    }

}