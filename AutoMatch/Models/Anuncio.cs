using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class Anuncio
    {
        [Key]
        public int Id_Anuncio { get; set; }

        [Required]
        public int Id_Vendedor { get; set; }

        [Required]
        public int Id_Admin { get; set; }

        [Required]
        public int Id_Modelo { get; set; }

        [Required, StringLength(50)]
        public string Titulo { get; set; }

        [Required, StringLength(50)]
        public string Descricao { get; set; }

        [Required]
        public DateTime Ano { get; set; }

        public int Preco { get; set; }
        public int Kilometros { get; set; }

        [Required, StringLength(50)]
        public string Localizacao { get; set; }

        public bool Estado { get; set; }

        [Required, StringLength(8)]
        public string Matricula { get; set; }

        public Vendedor Vendedor { get; set; }
        public Administrador Administrador { get; set; }
        public Modelo Modelo { get; set; }
    }
}
