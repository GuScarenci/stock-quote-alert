using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailService {
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public void SendAlert(string subject, string body) {
        var message = new MimeKit.MimeMessage();

        //Reading the settings safely to avoid nulls
        string smtpUser = _config["Settings:Smtp:User"] ?? "";
        string targetEmail = _config["Settings:TargetEmail"] ?? "";
        string smtpServer = _config["Settings:Smtp:Server"] ?? "smtp.gmail.com";
        string smtpPass = _config["Settings:Smtp:Pass"] ?? "";
        
        // Converts the door safely, using 587 as the default
        int smtpPort = int.TryParse(_config["Settings:Smtp:Port"], out var p) ? p : 587;

        message.From.Add(new MailboxAddress("Stock Alert", smtpUser));
        message.To.Add(new MailboxAddress("Investidor", targetEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        client.Connect(smtpServer, smtpPort, false);
        client.Authenticate(smtpUser, smtpPass);
        client.Send(message);
        client.Disconnect(true);
    }
}