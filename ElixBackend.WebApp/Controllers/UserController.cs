using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Helpers;
using ElixBackend.Business.IService;
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
    [HttpGet("[action]")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginRequestDto loginRequestDto)
    {
        try
        {
            var userDto = await userService.GetUserByEmailAsync(loginRequestDto.Email);
            if (userDto == null)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
                return View(loginRequestDto);
            }

            var passwordHasher = new PasswordHasher<UserDto>();
            var result = passwordHasher.VerifyHashedPassword(userDto, userDto.Password, loginRequestDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
                return View(loginRequestDto);
            }

            var jwtSecretKey = configuration["JwtSettings:SecretKey"];
            JwtTokenGenerator.GenerateAdminToken(userDto.Id, jwtSecretKey, out var jti);

            await tokenService.AddTokenAsync(jti, userDto.Id);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            return View(loginRequestDto);
        }
    }
}