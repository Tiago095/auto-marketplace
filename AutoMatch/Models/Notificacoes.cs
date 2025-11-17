using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Notificacoes
    {
        [Key]
        public int Id_notificacao { get; set; }

        [Required, ForeignKey("Vendedor")]
        public int Id_Vendedor { get; set; }

        [Required, ForeignKey("Comprador")]
        public int Id_Comprador { get; set; }

        [Required, StringLength(255)]
        public string Mensagem { get; set; }

        [Required]
        public DateTime Data_Envio { get; set; } = DateTime.Now;

        [Required]
        public bool Estado { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; }

        public Vendedor Vendedor { get; set; }
        public Comprador Comprador { get; set; }
    }
}
