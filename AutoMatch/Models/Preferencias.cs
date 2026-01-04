using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Preferencias
    {
        [Key]
        public int Preferencias_Id { get; set; }

        [Required]
        [ForeignKey("Comprador")]
        public int Id_Comprador { get; set; }

        [Required, StringLength(50)]
        public string Categoria { get; set; }

        [StringLength(100)]
        public string Detalhe { get; set; }

        public Comprador Comprador { get; set; }
    }
}
