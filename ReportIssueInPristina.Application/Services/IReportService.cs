using ReportIssueInPristina.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReportIssueInPristina.Application.Services
{
    public interface IReportService
    {
        Task<Issue> CreateReportAsync(Issue issue);
        Task<List<Issue>> GetAllReportsAsync();
        Task<List<Issue>> GetReportByUserAsync(string userId);
        Task<Issue?> GetReportByIdAsync(int id);
        Task<bool> UpdateStatusAsync(int issueId, string status);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
