using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ReportIssueInPristina.Domain.Models;
using System.Text.Json;

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
                .Property(i => i.ImageUrls)
                .HasColumnName("ImageUrls")
                .HasConversion(
                    urls => JsonSerializer.Serialize(urls, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value!.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    value => value!.ToList()));

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