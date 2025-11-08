using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models
{
    public class Modelo
    {
        [Key]
        public int Id_Modelo { get; set; }

        [Required, StringLength(50)]
        public string Marca { get; set; }

        [Required, StringLength(50)]
        public string NomeModelo { get; set; }

        [Required]
        public bool Transmissao { get; set; } // 0 - Manual, 1 - Automática

        [Required, StringLength(50)]
        public string Combustivel { get; set; }

        [Required, StringLength(50)]
        public string Categoria { get; set; }
    }
}
