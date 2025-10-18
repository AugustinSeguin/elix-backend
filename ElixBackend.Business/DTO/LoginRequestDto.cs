using System.ComponentModel.DataAnnotations;

namespace ElixBackend.Business.DTO
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "L'adresse e-mail est requise.")]
        [EmailAddress(ErrorMessage = "Le format de l'e-mail n'est pas valide.")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }
    }
}