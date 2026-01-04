using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Visita
    {
        [Key]
        public int Id_Visita { get; set; }

        [Required, ForeignKey("Reserva")]
        public int Id_Reserva { get; set; }

        [Required]
        public DateTime Data_Hora { get; set; }

        public Reserva Reserva { get; set; }
    }
}
