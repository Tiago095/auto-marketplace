using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models.ViewModel
{
    public class SellerFormViewModel
    {
        // Step 1 - Details
        [Required]
        [Display(Name = "Selling Type")]
        public string SellingType { get; set; } = "Individual"; // "Individual" ou "Professional"

        [Display(Name = "Document Number")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "The document number must have exactly 9 digits.")]
        public string? DocumentNumber { get; set; }

        // Telefone de contacto principal (campo obrigatório no formulário)
        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "The phone number must have exactly 9 digits.")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required]
        [Display(Name = "Preferred Contact Method")]
        public string? PreferredContactMethod { get; set; } = "Email"; // "Email", "Phone", "SMS"

        // Step 2 - Summary (apenas leitura)
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }

        // Tem de aceitar os termos para poder submeter
        [Required]
        [Display(Name = "I agree to the Seller Terms & Conditions")]
        public bool AcceptTerms { get; set; }
    }
}
