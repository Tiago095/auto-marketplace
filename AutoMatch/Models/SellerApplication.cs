using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoMatch.Models
{
    [Table("SellerApplications")]
    public class SellerApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SellingType { get; set; } // "Individual" or "Professional"

        [MaxLength(20)]
        public string DocumentNumber { get; set; } // NIF

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(10)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string PreferredContactMethod { get; set; }

        [Required]
        public bool AcceptTerms { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // "Pending", "Approved", "Rejected"

        public DateTime? ReviewedDate { get; set; }

        public int? ReviewedByAdminId { get; set; }

        [MaxLength(500)]
        public string RejectionReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Utilizador User { get; set; }

        [ForeignKey("ReviewedByAdminId")]
        public virtual Administrador ReviewedByAdmin { get; set; }
    }
}