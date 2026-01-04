using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Documento
    {
        [Key]
        public int Id_Doc { get; set; }

        [Required, ForeignKey("Anuncio")]
        public int Id_Anuncio { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; }

        [Required, StringLength(500)]
        public string CaminhoDocumento { get; set; }

        public Anuncio Anuncio { get; set; }
    }
}
