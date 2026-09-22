using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Domain.Models;
using ReportIssueInPristina.Web.Data;

namespace ReportIssueInPristina.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IImageStorageService _imageStorageService;

        public ReportService(
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IImageStorageService imageStorageService)
        {
            _contextFactory = contextFactory;
            _imageStorageService = imageStorageService;
        }

        public async Task<Issue> CreateReportAsync(Issue issue)
        {
            await using var db = _contextFactory.CreateDbContext();
            db.Issues.Add(issue);

            await db.SaveChangesAsync();
            return issue;
        }

        public async Task<List<Issue>> GetAllReportsAsync()
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.Issues
                .Include(i => i.Category)
                .OrderByDescending(i => i.DateCreated).ToListAsync();
        }

        public async Task<List<Issue>> GetReportByUserAsync(string userId)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.Issues
                .Include(i => i.Category)
                .Where(i => i.ApplicationUserId == userId)
                .OrderByDescending(i => i.DateCreated)
                .ToListAsync();
        }

        public async Task<Issue?> GetReportByIdAsync(int id)
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.Issues
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Issue?> UpdateReportAsync(Issue issue, string userId)
        {
            await using var db = _contextFactory.CreateDbContext();
            var existingIssue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id == issue.Id && i.ApplicationUserId == userId);

            if (existingIssue == null)
                return null;

            existingIssue.Title = issue.Title?.Trim();
            existingIssue.Description = issue.Description?.Trim();
            existingIssue.Location = issue.Location?.Trim();
            existingIssue.CategoryId = issue.CategoryId;
            existingIssue.ImageUrls = issue.ImageUrls;

            await db.SaveChangesAsync();
            return existingIssue;
        }

        public async Task<bool> DeleteReportAsync(int issueId, string userId, bool isAdmin)
        {
            await using var db = _contextFactory.CreateDbContext();
            var issue = await db.Issues.FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null || (!isAdmin && issue.ApplicationUserId != userId))
                return false;

            foreach (var imageUrl in issue.ImageUrls)
                await _imageStorageService.DeleteAsync(imageUrl);

            db.Issues.Remove(issue);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int issueId, string status, bool isAdmin)
        {
            if (!isAdmin)
                return false;

            var allowedStatuses = new[] { "Në pritje", "Në proces", "Zgjidhur" };
            if (!allowedStatuses.Contains(status))
                return false;

            await using var db = _contextFactory.CreateDbContext();
            var issue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
                return false;

            issue.Status = status;
            await db.SaveChangesAsync();

            return true;
        }


        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> CreateCategoryAsync(string name)
        {
            await using var db = _contextFactory.CreateDbContext();

            var normalizedName = name.Trim();
            var exists = await db.Categories
                .AnyAsync(c => c.Name != null && c.Name.ToLower() == normalizedName.ToLower());

            if (exists)
                throw new InvalidOperationException("Kjo kategori ekziston tashmë.");

            var category = new Category { Name = normalizedName };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return category;
        }
    }
}
