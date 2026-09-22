using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ReportIssueInPristina.Web.Data;
using System.Net;

namespace ReportIssueInPristina.Infrastructure.Services;

public sealed class IdentityEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;

    public IdentityEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsync(email, "Konfirmoni llogarinë tuaj", $"Për të konfirmuar llogarinë, klikoni <a href=\"{WebUtility.HtmlEncode(confirmationLink)}\">këtu</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsync(email, "Rivendosni fjalëkalimin", $"Për të rivendosur fjalëkalimin, klikoni <a href=\"{resetLink}\">këtu</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsync(email, "Kodi për rivendosjen e fjalëkalimit", $"Kodi juaj për rivendosjen e fjalëkalimit është: {resetCode}");

    private async Task SendAsync(string recipientEmail, string subject, string htmlBody)
    {
        var senderEmail = GetRequiredSetting("Email:SenderEmail");
        var senderName = _configuration["Email:SenderName"] ?? "Prishtina Civic Helper";
        var password = GetRequiredSetting("Email:Password");
        var host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.TryParse(_configuration["Email:SmtpPort"], out var configuredPort)
            ? configuredPort
            : 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(senderEmail, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    private string GetRequiredSetting(string key) =>
        _configuration[key]
        ?? throw new InvalidOperationException($"Email setting '{key}' is not configured.");
}
