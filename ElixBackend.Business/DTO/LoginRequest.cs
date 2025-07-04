using System.ComponentModel.DataAnnotations;

namespace ElixBackend.Business.DTO
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "L'email est requis")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        public string Password { get; set; } = string.Empty;
    }
}