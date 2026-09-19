using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReportIssueInPristina.Domain.Models;

namespace ReportIssueInPristina.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Issue> Issues { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Issue>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Issues)
                .HasForeignKey(c => c.CategoryId);


            builder.Entity<Issue>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Issues)
                .HasForeignKey(i => i.ApplicationUserId);
        }

    }
}