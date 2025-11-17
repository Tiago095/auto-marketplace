using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Administrador
    {
        [Required]
        public int Id_Admin { get; set; }

        [Key]
        [ForeignKey("Utilizador")]
        public int Id_User { get; set; }

        public Utilizador Utilizador { get; set; }
    }
}
