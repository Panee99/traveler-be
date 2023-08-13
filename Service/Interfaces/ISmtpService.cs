namespace Service.Interfaces;

public interface ISmtpService
{
    void MailAccountCredentials(string to, string name, string password, string role);
}