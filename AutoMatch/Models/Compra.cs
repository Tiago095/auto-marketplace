using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Compra
    {
        [Key]
        public int Id_Compra { get; set; }

        [Required, ForeignKey("Anuncio")]
        public int Id_Anuncio { get; set; }

        [Required, ForeignKey("Comprador")]
        public int Id_Comprador { get; set; }

        public DateTime Data_Compra { get; set; } = DateTime.Now;

        [Required]
        public bool Estado { get; set; }

        public Anuncio Anuncio { get; set; }
        public Comprador Comprador { get; set; }
    }
}
