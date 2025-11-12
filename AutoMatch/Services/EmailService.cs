using System.Net;
using System.Net.Mail;

namespace AutoMatch.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendContactEmailAsync(string fullName, string userEmail, string topic, string message)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            var host = smtpSettings["Host"] ?? "smtp.gmail.com";
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"] ?? "automatchtest@gmail.com";
            var password = smtpSettings["Password"] ?? "your-app-password";
            var recipientEmail = smtpSettings["RecipientEmail"] ?? "automatchtest@gmail.com";

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "AutoMatch Contact Form"),
                Subject = $"Contact Form: {topic}",
                Body = $@"
New contact form submission from AutoMatch:

Name: {fullName}
Email: {userEmail}
Topic: {topic}

Message:
{message}

---
This email was sent from the AutoMatch contact form.
Reply directly to this email to respond to: {userEmail}
",
                IsBodyHtml = false
            };

            mailMessage.To.Add(recipientEmail);
            mailMessage.ReplyToList.Add(new MailAddress(userEmail, fullName));

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Contact email sent successfully from {UserEmail}", userEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send contact email from {UserEmail}", userEmail);
            return false;
        }
    }
}
