using System.Security.Claims;
using System.Text.RegularExpressions;
using ElixBackend.API.Helpers;
using ElixBackend.Business.DTO;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ElixBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService, IConfiguration configuration, ITokenService tokenService)
        : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var userDto = await userService.GetUserByEmailAsync(loginRequestDto.Email);
            if (userDto == null)
            {
                return Unauthorized("Email ou mot de passe invalide.");
            }

            var passwordHasher = new PasswordHasher<UserDto>();
            var result = passwordHasher.VerifyHashedPassword(userDto, userDto.Password, loginRequestDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Email ou mot de passe invalide.");
            }
            string token;
            string jti;
            var jwtSecretKey = configuration["JwtSettings:SecretKey"];
            if(userDto.IsAdmin)
            { 
                token = JwtTokenGenerator.GenerateAdminToken(userDto.Id, jwtSecretKey, out jti);
                await tokenService.AddTokenAsync(jti, userDto.Id);
                return Ok(new { token });
            }
            token = JwtTokenGenerator.GenerateToken(userDto.Id.ToString(), jwtSecretKey, out jti);

            await tokenService.AddTokenAsync(jti, userDto.Id);

            return Ok(new { token });
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            var existingUser = await userService.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
            {
                return Conflict("Un utilisateur avec cet email existe déjà.");
            }

            if (userDto.Password != userDto.PasswordRepeated)
            {
                return BadRequest(new { message = "Les mots de passe ne correspondent pas." });
            }

            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!passwordRegex.IsMatch(userDto.Password))
            {
                return BadRequest(
                    "Le mot de passe doit contenir au minimum 8 caractères, dont une majuscule, une minuscule et un chiffre.");
            }

            var passwordHasher = new PasswordHasher<UserDto>();
            userDto.Password = passwordHasher.HashPassword(userDto, userDto.Password);

            var newUser = await userService.AddUserAsync(userDto);

            var jwtSecretKey = configuration["JwtSettings:SecretKey"];
            var token = JwtTokenGenerator.GenerateToken(newUser?.Id.ToString(), jwtSecretKey, out var jti);

            if (newUser == null)
            {
                return Problem("Erreur rencontrée lors de la création du compte", statusCode: 500);
            }

            await tokenService.AddTokenAsync(jti, newUser.Id);

            return Ok(new { token });
        }

        // GET: api/User/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userIdFromToken))
                return Unauthorized();

            if (id != userIdFromToken)
                return Forbid();

            var user = await userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        // PUT: api/User/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
                return BadRequest("L'ID dans l'URL doit correspondre à l'ID de l'utilisateur.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userIdFromToken))
                return Unauthorized();

            if (id != userIdFromToken)
                return Forbid();

            var existingUser = await userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            var user = await userService.UpdateUserAsync(userDto);
            return Ok(user);
        }

        // DELETE: api/User/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAsync(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userIdFromToken))
                return Unauthorized();

            if (id != userIdFromToken)
                return Forbid();

            var existingUser = await userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            await userService.DeleteUserAsync(id);
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (jti != null) await tokenService.RemoveTokenAsync(jti, userId);

            return Ok("Logged out.");
        }
    }
}