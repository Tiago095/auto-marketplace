using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Imagens
    {
        [Key]
        public int Id_Imagem { get; set; }

        [Required, ForeignKey("Anuncio")]
        public int Id_Anuncio { get; set; }

        [Required, StringLength(255)]
        public string CaminhoImagem { get; set; }

        public Anuncio Anuncio { get; set; }
    }
}
