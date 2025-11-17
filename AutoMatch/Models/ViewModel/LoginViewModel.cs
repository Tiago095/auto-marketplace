using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A password é obrigatória")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}