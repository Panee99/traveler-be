using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Interfaces;
using Service.Settings;

namespace Service.Implementations;

public class SmtpService : ISmtpService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<SmtpService> _logger;

    public SmtpService(IOptions<SmtpSettings> smtpSettings, ILogger<SmtpService> logger)
    {
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
    }

    public void MailAccountCredentials(string to, string name, string password, string role)
    {
        const string subject = "Traveling support application - Account information";
        var body = $@"
            <h2>Welcome to our platform, {name}!</h2>
            <p>Your account has been created successfully. Here are your details:</p>
            <p><strong>Name:</strong> {name}</p>
            <p><strong>Email:</strong> {to}</p>
            <p><strong>Password:</strong> {password}</p>
            <p><strong>Role:</strong> {role}</p>
            <p>Please keep this information secure and do not share it with others.</p>
            <p>Thank you for joining us!</p>
        ";

        Send(to, subject, body);

        _logger.LogInformation("Email sent to: {To}", to);
    }

    private void Send(string to, string subject, string body)
    {
        using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
        smtpClient.Credentials = new NetworkCredential(_smtpSettings.Email, _smtpSettings.Password);
        smtpClient.EnableSsl = true;

        try
        {
            var mailMessage = new MailMessage()
            {
                From = new MailAddress(_smtpSettings.Email),
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            };

            mailMessage.To.Add(to);

            smtpClient.Send(mailMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Message}", e.Message);
        }
    }
}