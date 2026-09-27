using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ReportIssueInPristina.Application.Services;

namespace ReportIssueInPristina.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendContactMessageAsync(string fullName, string email, string subject, string messageText)
        {
            var senderEmail = _configuration["Email:SenderEmail"]!;
            var senderName = _configuration["Email:SenderName"] ?? "Prishtina Civic Helper";
            var password = _configuration["Email:Password"];
            var recipientEmail = _configuration["Email:RecipientEmail"]!;

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(senderName, senderEmail));

            message.To.Add(MailboxAddress.Parse(recipientEmail));

            //reply to the person that contacted you

            message.ReplyTo.Add(MailboxAddress.Parse(email));
            message.Subject = $"Prishtina Civic Helper - {subject}";

            message.Body = new TextPart("plain")
            {
                Text = $"""
                New message from Prishtina Civic Helper

                Name:{fullName}
                Email:{email}
                Subject:{subject}

                Message:{messageText}
                """

            };

            using var client = new SmtpClient();

            await client.ConnectAsync("smtp.gmail.com",
                587,
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(senderEmail,
                password);

            await client.SendAsync(message);

            await client.DisconnectAsync(true);
        }

        public async Task SendStatusUpdateAsync(string recipientEmail, string issueTitle, string previousStatus, string newStatus, string? note)
        {
            var senderEmail = _configuration["Email:SenderEmail"]!;
            var senderName = _configuration["Email:SenderName"] ?? "Prishtina Civic Helper";
            var password = _configuration["Email:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(recipientEmail));
            message.Subject = $"Prishtina Civic Helper - Përditësim i raportimit: {issueTitle}";
            message.Body = new TextPart("plain")
            {
                Text = $"""
                Statusi i raportimit tuaj është përditësuar.

                Raportimi: {issueTitle}
                Statusi i mëparshëm: {previousStatus}
                Statusi i ri: {newStatus}
                Shënim: {note ?? "Pa shënim"}
                """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

