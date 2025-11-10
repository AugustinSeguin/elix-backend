using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Helpers;
using ElixBackend.Business.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;

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
            var token = JwtTokenGenerator.GenerateAdminToken(userDto.Id, jwtSecretKey, out var jti);

            await tokenService.AddTokenAsync(jti, userDto.Id);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps, 
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            };
            Response.Cookies.Append("JwtToken", token, cookieOptions);

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt =  handler.ReadJwtToken(token);

            var jwtClaims = jwt.Claims.Select(c => new Claim(c.Type, c.Value)).ToList();

            if (jwtClaims.All(c => c.Type != ClaimTypes.Name) && !string.IsNullOrEmpty(userDto.Username))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Name, userDto.Username));
            }
            else if (jwtClaims.All(c => c.Type != ClaimTypes.Name) && !string.IsNullOrEmpty(userDto.Email))
            {
                jwtClaims.Add(new Claim(ClaimTypes.Name, userDto.Email));
            }

            var identity = new ClaimsIdentity(jwtClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Email ou mot de passe invalide.");
            return View(loginRequestDto);
        }
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("JwtToken", out var token) && !string.IsNullOrEmpty(token))
        {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
                var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(jti) && int.TryParse(userIdClaim, out var userId))
                {
                    await tokenService.RemoveTokenAsync(jti, userId);
                }
            Response.Cookies.Delete("JwtToken");
        }

        // Sign out of cookie authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "User");
    }
}