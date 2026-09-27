using System;
using System.Collections.Generic;
using System.Text;

namespace ReportIssueInPristina.Application.Services
{
    public interface IEmailService
    {
        Task SendContactMessageAsync(string fullName, string email, string subject, string message);
        Task SendStatusUpdateAsync(string recipientEmail, string issueTitle, string previousStatus, string newStatus, string? note);
    }
}
