using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Reserva
    {
        [Key]
        public int Id_Reserva { get; set; }

        [Required, ForeignKey("Comprador")]
        public int Id_Comprador { get; set; }

        [Required, ForeignKey("Anuncio")]
        public int Id_Anuncio { get; set; }

        [Required]
        public DateTime Data_Inicio { get; set; }

        [Required]
        public DateTime Data_Fim { get; set; }

        [Required]
        public bool Estado { get; set; }

        public Comprador Comprador { get; set; }
        public Anuncio Anuncio { get; set; }
    }
}
