using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models
{
    public class Utilizador
    {
        [Key]
        public int Id_User { get; set; }

        [Required]
        public bool Estado { get; set; }

        [Required, StringLength(50)]
        public string Nome { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(60)]
        [EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string Senha { get; set; }
    }
}
