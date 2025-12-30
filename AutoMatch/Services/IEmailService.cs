namespace AutoMatch.Services;

public interface IEmailService
{
    Task<bool> SendContactEmailAsync(string fullName, string userEmail, string topic, string message);
    Task<bool> SendPurchaseConfirmationAsync(string userEmail, string subject, string body);
}
