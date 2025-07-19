using Microsoft.AspNetCore.Mvc;
using ElixBackend.API.Helpers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ElixBackend.WebApp.Controllers;

[Route("[controller]")]
public class UserController(
    IUserService userService,
    IConfiguration configuration,
    ITokenService tokenService)
    : Controller
{
    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            return View(loginRequest);
        }

        try
        {
            var user = await userService.GetUserByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
                return View(loginRequest);
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
                return View(loginRequest);
            }

            var jwtSecretKey = configuration["JwtSettings:SecretKey"];
            JwtTokenGenerator.GenerateAdminToken(user.Id, jwtSecretKey, out var jti);

            await tokenService.AddTokenAsync(jti, user.Id);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            return View(loginRequest);
        }
    }
}