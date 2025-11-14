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
            JwtSecurityToken jwt = handler.ReadJwtToken(token);

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

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "User");
    }

    [HttpGet("[action]")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var users = await userService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet("[action]")]
    [Authorize]
    public IActionResult Create()
    {
        return View(new UserDto
        {
            Username = "",
            Email = "",
            Password = "",
            PasswordRepeated = "",
            Firstname = "",
            Lastname = ""
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(
        [Bind(
            "Username,Email,Password,PasswordRepeated,Firstname,Lastname,PhoneNumber,Birthdate,Gender,IsAdmin,IsPremium")]
        UserDto userDto)
    {
        ModelState.Remove("IsAdmin");
        ModelState.Remove("IsPremium");

        var isAdminValue = Request.Form["IsAdmin"].ToString();
        userDto.IsAdmin = isAdminValue.Contains("true");

        var isPremiumValue = Request.Form["IsPremium"].ToString();
        userDto.IsPremium = isPremiumValue.Contains("true");

        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        try
        {
            var passwordHasher = new PasswordHasher<UserDto>();
            userDto.Password = passwordHasher.HashPassword(userDto, userDto.Password);

            await userService.AddUserAsync(userDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de créer l'utilisateur. Veuillez réessayer.");
            return View(userDto);
        }
    }

    [HttpGet("[action]")]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(UserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.Password))
        {
            var user = await userService.GetUserByIdAsync(userDto.Id);
            if (user == null)
            {
                return NotFound();
            }

            userDto.Password = user.Password;
            userDto.PasswordRepeated = user.PasswordRepeated;
            ModelState.Remove("IsValid");
        }

        ModelState.Remove("IsAdmin");
        ModelState.Remove("IsPremium");

        var isAdminValue = Request.Form["IsAdmin"].ToString();
        userDto.IsAdmin = isAdminValue.Contains("true");

        var isPremiumValue = Request.Form["IsPremium"].ToString();
        userDto.IsPremium = isPremiumValue.Contains("true");
        
        try
        {
            await userService.UpdateUserAsync(userDto);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour l'utilisateur. Veuillez réessayer.");
            return View(userDto);
        }
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await userService.DeleteUserAsync(id);
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            return RedirectToAction("Index");
        }
    }
}