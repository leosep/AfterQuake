using System.Net;
using System.Net.Mail;

namespace AfterQuake.Web.Services;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendAsync(string to, string subject, string body)
    {
        var smtpHost = _config["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "25");
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        using var msg = new MailMessage("noreply@afterquake.com.do", to, subject, body) { IsBodyHtml = true };
        using var client = new SmtpClient(smtpHost, smtpPort);
        if (!string.IsNullOrEmpty(username))
            client.Credentials = new NetworkCredential(username, password);
        client.EnableSsl = bool.Parse(_config["Email:EnableSsl"] ?? "false");
        await client.SendMailAsync(msg);
    }
}
