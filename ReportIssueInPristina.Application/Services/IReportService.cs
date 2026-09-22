using ReportIssueInPristina.Domain.Models;
using System.Collections.Generic;

namespace ReportIssueInPristina.Application.Services
{
    public interface IReportService
    {
        Task<Issue> CreateReportAsync(Issue issue);
        Task<List<Issue>> GetAllReportsAsync();
        Task<List<Issue>> GetReportByUserAsync(string userId);
        Task<Issue?> GetReportByIdAsync(int id);
        Task<Issue?> UpdateReportAsync(Issue issue, string userId);
        Task<bool> DeleteReportAsync(int issueId, string userId, bool isAdmin);
        Task<bool> UpdateStatusAsync(int issueId, string status, bool isAdmin);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category> CreateCategoryAsync(string name);
    }

    public interface IImageStorageService
    {
        Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
