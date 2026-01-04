using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AutoMatch.ViewModels
{
    public class EditListingViewModel
    {
        public int Id_Anuncio { get; set; }

        // Campos apenas para leitura (Modelo)
        public string Marca { get; set; }
        public string NomeModelo { get; set; }
        public string Matricula { get; set; }
        public bool Transmissao { get; set; } // "Manual" / "Automática"
        public int Ano { get; set; }
        public string Combustivel { get; set; }
        public string Categoria { get; set; }

        // Campos editáveis (Anuncio)
        public int Preco { get; set; }
        public int Kilometros { get; set; }
        public string Descricao { get; set; }
        public string Localizacao { get; set; }

        // Imagens
        public List<string> ImagensExistentes { get; set; } = new();
        public List<IFormFile> NovasImagens { get; set; } = new();
        public List<string> ImagensParaDeletar { get; set; } = new();
    }
}
