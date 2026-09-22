using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Domain.Models;
using ReportIssueInPristina.Web.Data;

namespace ReportIssueInPristina.Infrastructure.Services
{
   public class ReportService: IReportService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public ReportService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Issue> CreateReportAsync(Issue issue)
        {
            await using var db = _contextFactory.CreateDbContext();
            db.Issues.Add(issue);

            await db.SaveChangesAsync();
            return issue;
        }

        public async Task<List<Issue>>GetAllReportsAsync()
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
                .OrderByDescending(i =>i.DateCreated)
                .ToListAsync();
        }

        public async Task<Issue?> GetReportByIdAsync(int id) 
        {
            await using var db = _contextFactory.CreateDbContext();
            return await db.Issues
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i =>i.Id ==id);
        }

        public async Task<bool> UpdateStatusAsync(int issueId, string status)
        {
            await using var db = _contextFactory.CreateDbContext();
            var issue = await db.Issues
                .FirstOrDefaultAsync(i => i.Id ==issueId);

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
    }
}
