using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ElixBackend.Business.IService;
using ElixBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using ElixBackend.API.Helpers;
using ElixBackend.Business.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using LoginRequest = ElixBackend.Business.DTO.LoginRequest;

namespace ElixBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, IConfiguration configuration, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var user = await _userService.GetUserByEmailAsync(loginRequest.Email);
            if (user == null)
            {
                return Unauthorized("Email ou mot de passe invalide.");
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Email ou mot de passe invalide.");
            }

            var jwtSecretKey = _configuration["JwtSettings:SecretKey"];
            var token = JwtTokenGenerator.GenerateToken(user.Id.ToString(), jwtSecretKey, out var jti);
            
            await _tokenService.AddTokenAsync(jti, (int)user.Id);

            return Ok(new { token });
        }

        // POST: api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var existingUser = await _userService.GetUserByEmailAsync(userDTO.Email);
            if (existingUser != null)
            {
                return Conflict("Un utilisateur avec cet email existe déjà.");
            }
            
            var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
            if (!passwordRegex.IsMatch(userDTO.Password))
            {
                return BadRequest("Le mot de passe doit contenir au minimum 8 caractères, dont une majuscule, une minuscule et un chiffre.");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            userDTO.Password = hashedPassword;

            var newUser = await _userService.AddUserAsync(userDTO);
            
            var jwtSecretKey = _configuration["JwtSettings:SecretKey"];
            var token = JwtTokenGenerator.GenerateToken(newUser.Id.ToString(), jwtSecretKey, out var jti);
            
            await _tokenService.AddTokenAsync(jti, (int)newUser.Id);
            
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

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            
            return Ok(user);
        }

        // PUT: api/User/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserDTO userDTO)
        {
            if (id != userDTO.Id)
                return BadRequest("L'ID dans l'URL doit correspondre à l'ID de l'utilisateur.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userIdFromToken))
                return Unauthorized();

            if (id != userIdFromToken)
                return Forbid(); 
            
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            await _userService.UpdateUserAsync(userDTO);
            return NoContent();
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
            
            var existingUser = await _userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            await _tokenService.RemoveTokenAsync(jti, userId);

            return Ok("Logged out.");
        }
    }
}
