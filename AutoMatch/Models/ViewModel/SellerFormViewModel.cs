using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models.ViewModel
{
    public class SellerFormViewModel
    {
        [Required]
        [Display(Name = "Selling Type")]
        public string SellingType { get; set; } = "Individual";

        [Display(Name = "Document Number")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "The document number must have exactly 9 digits.")]
        public string? DocumentNumber { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "The phone number must have exactly 9 digits.")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Required]
        [Display(Name = "Preferred Contact Method")]
        public string? PreferredContactMethod { get; set; } = "Email"; 

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }

        [Required]
        [Display(Name = "I agree to the Seller Terms & Conditions")]
        public bool AcceptTerms { get; set; }
    }
}
