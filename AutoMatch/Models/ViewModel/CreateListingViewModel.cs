using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoMatch.Models.ViewModel
{
    public class CreateListingViewModel
    {
        [Required]
        public int IdModelo { get; set; }

        [Display(Name = "Brand")]
        public string Marca { get; set; }

        [Display(Name = "Model")]
        public string NomeModelo { get; set; }

        [Display(Name = "Transmission")]
        public string Transmissao { get; set; }

        [Display(Name = "Combustivel")]
        public string Combustivel { get; set; }

        [Display(Name = "Categoria")]
        public string Categoria { get; set; }

        // Dados do anúncio
        [Required]
        public string Titulo { get; set; }
        [Required]
        public string Descricao { get; set; }
        [Required]
        public int Ano { get; set; }
        [Required]
        public int Preco { get; set; }
        [Required]
        public int Kilometros { get; set; }
        [Required]
        public string Localizacao { get; set; }
        [Required]
        public string Matricula { get; set; }

        // Uploads
        [Required]
        public List<IFormFile> Imagens { get; set; }
        public List<IFormFile> Documentos { get; set; }
    }

}
