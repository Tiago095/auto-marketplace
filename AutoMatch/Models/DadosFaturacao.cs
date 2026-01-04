using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    public class DadosFaturacao
    {
        [Key]
        public int Dados_Faturacao_Id { get; set; }

        [Required]
        [ForeignKey("Vendedor")]
        public int Id_Vendedor { get; set; }

        [StringLength(100)]
        public string Fatura { get; set; }

        public int Valor { get; set; }

        public Vendedor Vendedor { get; set; }
    }
}
