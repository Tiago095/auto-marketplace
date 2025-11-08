using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models
{
    public class CodigoPostal
    {
        [Key, StringLength(8)]
        public string Codigo_Postal { get; set; }

        [Required, StringLength(100)]
        public string Localidade { get; set; }
    }
}
