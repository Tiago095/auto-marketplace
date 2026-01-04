using System.ComponentModel.DataAnnotations;

namespace AutoMatch.Models;

public class ContactFormModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Topic is required")]
    [StringLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
    public string Topic { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;
}
