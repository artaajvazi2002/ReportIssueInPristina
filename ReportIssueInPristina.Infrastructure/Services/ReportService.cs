using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Domain.Models;
using ReportIssueInPristina.Web.Data;

namespace ReportIssueInPristina.Infrastructure.Services
{
   public class ReportService: IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Issue> CreateReportAsync(Issue issue)
        {
            _context.Issues.Add(issue);

            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task<List<Issue>>GetAllReportsAsync()
        {
            return await _context.Issues
                .Include(i => i.Category)
                .OrderByDescending(i => i.DateCreated).ToListAsync();
        }

        public async Task<List<Issue>> GetReportByUserAsync(string userId) 
        { 
            return await _context.Issues
                .Include(i => i.Category)
                .Where(i => i.ApplicationUserId == userId)
                .OrderByDescending(i =>i.DateCreated)
                .ToListAsync();
        }

        public async Task<Issue?> GetReportByIdAsync(int id) 
        {
            return await _context.Issues
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i =>i.Id ==id);
        }

        public async Task<bool> UpdateStatusAsync(int issueId, string status)
        {
            var issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id ==issueId);

            if (issue == null)
                return false;

            issue.Status = status;
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
