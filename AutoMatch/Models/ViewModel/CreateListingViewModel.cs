using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models.ViewModel
{
    public class CreateListingViewModel
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Brand")]
        public string Marca { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Model")]
        public string Modelo { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        [Required]
        [Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string Tipo { get; set; }

        [Range(0, int.MaxValue)]
        public int? Kilometros { get; set; }

        [Required]
        [StringLength(50)]
        public string Localizacao { get; set; }

        [Required]
        [StringLength(200)]
        public string Descricao { get; set; }

        // Informação adicional para suporte de documentos

        [StringLength(17)]
        [Display(Name = "VIN / Nº de Chassis")]
        public string? Vin { get; set; }

        [Display(Name = "Nº de Proprietários")]
        [Range(1, 20)]
        public int? NumeroProprietarios { get; set; }

        [Display(Name = "Inspeção válida até")]
        [DataType(DataType.Date)]
        public DateTime? InspecaoValidaAte { get; set; }

        [Display(Name = "Tem registo de manutenção / livro de revisões")]
        public bool TemHistoricoManutencao { get; set; }

        [Display(Name = "Documentos que vai fornecer")]
        public bool DocRegistoPropriedade { get; set; } // Documento único automóvel / registo

        [Display(Name = "Documento da inspeção")]
        public bool DocInspecao { get; set; }

        [Display(Name = "Apólice de seguro")]
        public bool DocSeguro { get; set; }
    }
}
