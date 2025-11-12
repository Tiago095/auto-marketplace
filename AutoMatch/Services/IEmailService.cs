namespace AutoMatch.Services;

public interface IEmailService
{
    Task<bool> SendContactEmailAsync(string fullName, string userEmail, string topic, string message);
}
