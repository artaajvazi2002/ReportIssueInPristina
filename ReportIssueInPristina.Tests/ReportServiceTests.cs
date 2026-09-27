using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Domain.Models;
using ReportIssueInPristina.Infrastructure.Services;
using ReportIssueInPristina.Web.Data;
using Xunit;

namespace ReportIssueInPristina.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task CreateReportAsync_PersistsAndReturnsReport()
    {
        var (service, factory, _, _) = CreateService();
        var issue = new Issue
        {
            Title = "Pothole",
            ApplicationUserId = "user-1",
            Latitude = 42.6629,
            Longitude = 21.1655
        };

        var created = await service.CreateReportAsync(issue);

        created.Should().BeSameAs(issue);
        created.Id.Should().BePositive();
        await using var verifyDb = factory.CreateDbContext();
        (await verifyDb.Issues.SingleAsync()).Title.Should().Be("Pothole");
        var savedIssue = await verifyDb.Issues.SingleAsync();
        savedIssue.Latitude.Should().Be(42.6629);
        savedIssue.Longitude.Should().Be(21.1655);
    }

    [Fact]
    public async Task GetAllReportsAsync_ReturnsReportsNewestFirstWithCategory()
    {
        var (service, factory, _, _) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            var category = new Category { Name = "Roads" };
            db.Categories.Add(category);
            db.Issues.AddRange(
                new Issue { Title = "Older", DateCreated = new DateTime(2025, 1, 1), Category = category },
                new Issue { Title = "Newer", DateCreated = new DateTime(2025, 2, 1), Category = category });
            await db.SaveChangesAsync();
        }

        var reports = await service.GetAllReportsAsync();

        reports.Select(report => report.Title).Should().Equal("Newer", "Older");
        reports[0].Category!.Name.Should().Be("Roads");
    }

    [Fact]
    public async Task GetReportByIdAsync_ReturnsMatchingReportWithCategoryOrNull()
    {
        var (service, factory, _, _) = CreateService();
        int issueId;
        await using (var db = factory.CreateDbContext())
        {
            var category = new Category { Name = "Roads" };
            var issue = new Issue { Title = "Pothole", Category = category };
            db.Issues.Add(issue);
            await db.SaveChangesAsync();
            issueId = issue.Id;
        }

        var found = await service.GetReportByIdAsync(issueId);
        var missing = await service.GetReportByIdAsync(issueId + 1);

        found!.Title.Should().Be("Pothole");
        found.Category!.Name.Should().Be("Roads");
        missing.Should().BeNull();
    }

    [Fact]
    public async Task GetReportByUserAsync_ReturnsOnlyThatUsersReportsNewestFirst()
    {
        var (service, factory, _, _) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            var category = new Category { Name = "Roads" };
            db.Categories.Add(category);
            db.Issues.AddRange(
                new Issue { Title = "Mine older", ApplicationUserId = "user-1", DateCreated = new DateTime(2025, 1, 1), Category = category },
                new Issue { Title = "Theirs", ApplicationUserId = "user-2", DateCreated = new DateTime(2025, 3, 1), Category = category },
                new Issue { Title = "Mine newer", ApplicationUserId = "user-1", DateCreated = new DateTime(2025, 2, 1), Category = category });
            await db.SaveChangesAsync();
        }

        var reports = await service.GetReportByUserAsync("user-1");

        reports.Select(report => report.Title).Should().Equal("Mine newer", "Mine older");
    }

    [Fact]
    public async Task UpdateReportAsync_TrimsFieldsAndPreservesOtherFields()
    {
        var (service, factory, _, _) = CreateService();
        int issueId;
        await using (var db = factory.CreateDbContext())
        {
            var issue = new Issue
            {
                Title = "Original",
                Description = "Original description",
                Location = "Original location",
                Status = "Në pritje",
                ApplicationUserId = "owner"
            };
            db.Issues.Add(issue);
            await db.SaveChangesAsync();
            issueId = issue.Id;
        }

        var updated = await service.UpdateReportAsync(new Issue
        {
            Id = issueId,
            Title = "  Updated  ",
            Description = "  Details  ",
            Location = "  Center  "
        }, "owner");

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated");
        updated.Description.Should().Be("Details");
        updated.Location.Should().Be("Center");
        updated.Status.Should().Be("Në pritje");
    }

    [Fact]
    public async Task UpdateReportAsync_ReturnsNullForAnotherUsersReport()
    {
        var (service, factory, _, _) = CreateService();
        int issueId;
        await using (var db = factory.CreateDbContext())
        {
            var issue = new Issue { Title = "Original", ApplicationUserId = "owner" };
            db.Issues.Add(issue);
            await db.SaveChangesAsync();
            issueId = issue.Id;
        }

        var updated = await service.UpdateReportAsync(new Issue { Id = issueId, Title = "Changed" }, "intruder");

        updated.Should().BeNull();
        await using var verifyDb = factory.CreateDbContext();
        (await verifyDb.Issues.SingleAsync()).Title.Should().Be("Original");
    }

    [Fact]
    public async Task DeleteReportAsync_RejectsNonOwnerWithoutDeletingImages()
    {
        var (service, factory, imageStorage, _) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            db.Issues.Add(new Issue { Title = "Protected", ApplicationUserId = "owner", ImageUrls = ["image-url"] });
            await db.SaveChangesAsync();
        }

        var deleted = await service.DeleteReportAsync(1, "intruder", isAdmin: false);

        deleted.Should().BeFalse();
        imageStorage.DeletedUrls.Should().BeEmpty();
        await using var verifyDb = factory.CreateDbContext();
        (await verifyDb.Issues.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task DeleteReportAsync_AllowsAdminAndDeletesImages()
    {
        var (service, factory, imageStorage, _) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            db.Issues.Add(new Issue { Title = "Report", ApplicationUserId = "owner", ImageUrls = ["image-1", "image-2"] });
            await db.SaveChangesAsync();
        }

        var deleted = await service.DeleteReportAsync(1, "admin", isAdmin: true);

        deleted.Should().BeTrue();
        imageStorage.DeletedUrls.Should().Equal("image-1", "image-2");
        await using var verifyDb = factory.CreateDbContext();
        (await verifyDb.Issues.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UpdateStatusAsync_RequiresAdminAndAnAllowedStatus()
    {
        var (service, factory, _, emailService) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser { Id = "reporter", Email = "reporter@example.com", UserName = "reporter@example.com" });
            db.Issues.Add(new Issue { Title = "Report", Status = "Në pritje", ApplicationUserId = "reporter" });
            await db.SaveChangesAsync();
        }

        (await service.UpdateStatusAsync(1, "Zgjidhur", isAdmin: false, "admin-id", "Admin", null)).Should().BeFalse();
        (await service.UpdateStatusAsync(1, "Unknown", isAdmin: true, "admin-id", "Admin", null)).Should().BeFalse();
        (await service.UpdateStatusAsync(1, "Zgjidhur", isAdmin: true, "admin-id", "Admin", "  Repaired  ")).Should().BeTrue();
        (await service.UpdateStatusAsync(1, "Zgjidhur", isAdmin: true, "admin-id", "Admin", "No change")).Should().BeTrue();

        await using var verifyDb = factory.CreateDbContext();
        (await verifyDb.Issues.SingleAsync()).Status.Should().Be("Zgjidhur");
        var history = await service.GetStatusHistoryAsync(1);
        history.Should().ContainSingle();
        history[0].PreviousStatus.Should().Be("Në pritje");
        history[0].NewStatus.Should().Be("Zgjidhur");
        history[0].ChangedByUserId.Should().Be("admin-id");
        history[0].ChangedByName.Should().Be("Admin");
        history[0].Note.Should().Be("Repaired");
        history[0].ChangedAt.Should().NotBe(default);
        emailService.Messages.Should().ContainSingle();
        emailService.Messages[0].RecipientEmail.Should().Be("reporter@example.com");
        emailService.Messages[0].NewStatus.Should().Be("Zgjidhur");
        emailService.Messages[0].Note.Should().Be("Repaired");
    }

    [Fact]
    public async Task CreateCategoryAsync_TrimsNameAndRejectsCaseInsensitiveDuplicates()
    {
        var (service, _, _, _) = CreateService();

        var category = await service.CreateCategoryAsync("  Roads  ");

        category.Name.Should().Be("Roads");
        var act = () => service.CreateCategoryAsync("roads");
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Kjo kategori ekziston tashmë.");
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsCategoriesAlphabetically()
    {
        var (service, factory, _, _) = CreateService();
        await using (var db = factory.CreateDbContext())
        {
            db.Categories.AddRange(new Category { Name = "Roads" }, new Category { Name = "Parks" });
            await db.SaveChangesAsync();
        }

        var categories = await service.GetAllCategoriesAsync();

        categories.Select(category => category.Name).Should().Equal("Parks", "Roads");
    }

    private static (ReportService Service, TestDbContextFactory Factory, FakeImageStorageService ImageStorage, FakeEmailService EmailService) CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var factory = new TestDbContextFactory(options);
        var imageStorage = new FakeImageStorageService();
        var emailService = new FakeEmailService();
        return (new ReportService(factory, imageStorage, emailService), factory, imageStorage, emailService);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => new(options);
    }

    private sealed class FakeImageStorageService : IImageStorageService
    {
        public List<string> DeletedUrls { get; } = [];

        public Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult(fileName);

        public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
        {
            DeletedUrls.Add(imageUrl);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEmailService : IEmailService
    {
        public List<StatusMessage> Messages { get; } = [];

        public Task SendContactMessageAsync(string fullName, string email, string subject, string message)
            => Task.CompletedTask;

        public Task SendStatusUpdateAsync(string recipientEmail, string issueTitle, string previousStatus, string newStatus, string? note)
        {
            Messages.Add(new StatusMessage(recipientEmail, issueTitle, previousStatus, newStatus, note));
            return Task.CompletedTask;
        }

        public sealed record StatusMessage(string RecipientEmail, string IssueTitle, string PreviousStatus, string NewStatus, string? Note);
    }
}