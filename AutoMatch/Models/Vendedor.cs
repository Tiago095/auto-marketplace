using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Vendedor
    {
        [Key]
        [ForeignKey("Utilizador")]
        public int Id_User { get; set; }

        [Required]
        public bool Tipo { get; set; }

        public int? NIF { get; set; }

        [Required, StringLength(100)]
        public string Contactos { get; set; }

        [Required, StringLength(50)]
        public string Rua { get; set; }

        [Required, StringLength(8)]
        public string Codigo_Postal { get; set; }

        public Utilizador Utilizador { get; set; }
        public CodigoPostal CodigoPostal { get; set; }

        // IMPORTANTE: isto evita erros no Anuncio
        public ICollection<Anuncio> Anuncios { get; set; }
    }
}
