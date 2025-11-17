using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    [Table("Compradores")]
    public class Comprador
    {
        [Key]
        [ForeignKey("Utilizador")]
        public int Id_User { get; set; }

        [Required, StringLength(100)]
        public string Contactos { get; set; }

        [Required, StringLength(50)]
        public string Rua { get; set; }

        [Required, StringLength(8)]
        [ForeignKey("CodigoPostal")]
        public string Codigo_Postal { get; set; }

        public Utilizador Utilizador { get; set; }
        public CodigoPostal CodigoPostal { get; set; }
    }
}
