using System.Security.Claims;
using System.Text.RegularExpressions;
using ElixBackend.Business.DTO;
using ElixBackend.Business.Helpers;
using ElixBackend.Business.IService;
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

            var storedHash = userDto.Password; 

            var result = passwordHasher.VerifyHashedPassword(
                userDto, 
                storedHash, 
                loginRequestDto.Password 
            );

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

            // Convertir la date de naissance en UTC si nécessaire (PostgreSQL exige UTC)
            if (userDto.Birthdate.HasValue && userDto.Birthdate.Value.Kind == DateTimeKind.Unspecified)
            {
                userDto.Birthdate = DateTime.SpecifyKind(userDto.Birthdate.Value, DateTimeKind.Utc);
            }
            else if (userDto.Birthdate.HasValue && userDto.Birthdate.Value.Kind == DateTimeKind.Local)
            {
                userDto.Birthdate = userDto.Birthdate.Value.ToUniversalTime();
            }

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

            // Convertir le chemin de l'image en URL complète
            if (!string.IsNullOrWhiteSpace(user.PictureMediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(user.PictureMediaPath);
                user.PictureMediaPath = $"{baseUrl}/uploads/{fileName}";
            }

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

            // Conserver le chemin de l'image existante si non fourni
            if (string.IsNullOrWhiteSpace(userDto.PictureMediaPath))
            {
                userDto.PictureMediaPath = existingUser.PictureMediaPath;
            }

            // Gestion du mot de passe
            if (string.IsNullOrWhiteSpace(userDto.Password))
            {
                // Pas de nouveau mot de passe fourni : conserver l'existant
                userDto.Password = existingUser.Password;
                userDto.PasswordRepeated = existingUser.Password;
            }
            else
            {
                // Nouveau mot de passe fourni : vérifier et hasher
                if (userDto.Password != userDto.PasswordRepeated)
                {
                    return BadRequest(new { message = "Les mots de passe ne correspondent pas." });
                }

                var passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
                if (!passwordRegex.IsMatch(userDto.Password))
                {
                    return BadRequest("Le mot de passe doit contenir au minimum 8 caractères, dont une majuscule, une minuscule et un chiffre.");
                }

                var passwordHasher = new PasswordHasher<UserDto>();
                userDto.Password = passwordHasher.HashPassword(userDto, userDto.Password);
                userDto.PasswordRepeated = userDto.Password;
            }

            // Convertir la date de naissance en UTC si nécessaire (PostgreSQL exige UTC)
            if (userDto.Birthdate.HasValue && userDto.Birthdate.Value.Kind == DateTimeKind.Unspecified)
            {
                userDto.Birthdate = DateTime.SpecifyKind(userDto.Birthdate.Value, DateTimeKind.Utc);
            }
            else if (userDto.Birthdate.HasValue && userDto.Birthdate.Value.Kind == DateTimeKind.Local)
            {
                userDto.Birthdate = userDto.Birthdate.Value.ToUniversalTime();
            }

            var user = await userService.UpdateUserAsync(userDto);
            return Ok(user);
        }

        // PUT: api/User/{id}/picture
        [Authorize]
        [HttpPut("{id}/picture")]
        public async Task<IActionResult> UpdateUserPictureAsync(int id, IFormFile pictureFile)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userIdFromToken))
                return Unauthorized();

            if (id != userIdFromToken)
                return Forbid();

            var existingUser = await userService.GetUserByIdAsync(id);
            if (existingUser == null)
                return NotFound();

            if (pictureFile is not { Length: > 0 })
                return BadRequest("Aucun fichier fourni.");

            var savedPath = await MediaHelper.HandleMediaUploadAsync(pictureFile, configuration, existingUser.PictureMediaPath);
            existingUser.PictureMediaPath = savedPath;

            var user = await userService.UpdateUserAsync(existingUser);

            // return url
            if (!string.IsNullOrWhiteSpace(user?.PictureMediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(user.PictureMediaPath);
                user.PictureMediaPath = $"{baseUrl}/uploads/{fileName}";
            }

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

        // GET: api/User/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await userService.GetMeAsync(userId);
            if (user == null)
                return NotFound();

            // Convertir le chemin de l'image en URL complète
            if (!string.IsNullOrWhiteSpace(user.PictureMediaPath))
            {
                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";
                var fileName = Path.GetFileName(user.PictureMediaPath);
                user.PictureMediaPath = $"{baseUrl}/uploads/{fileName}";
            }

            return Ok(user);
        }

        // GET: /user/{userId}/{fileName}
        [HttpGet("/user/{userId}/{fileName}")]
        public IActionResult GetUserPicture(int userId, string fileName)
        {
            try
            {
                var uploadsPath = configuration["FileStorage:UploadsPath"] ?? "wwwroot/uploads";
                var uploadsFolder = Path.IsPathRooted(uploadsPath) 
                    ? uploadsPath 
                    : Path.Combine(Directory.GetCurrentDirectory(), uploadsPath);
                
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Image non trouvée.");
                }

                var contentType = "image/jpeg";
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                var imageBytes = System.IO.File.ReadAllBytes(filePath);
                return File(imageBytes, contentType);
            }
            catch (Exception)
            {
                return NotFound("Erreur lors de la récupération de l'image.");
            }
        }
    }
}