using System.Security.Claims;
using AwesomeAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ReportIssueInPristina.Web.Data;
using ReportIssueInPristina.Web.Services;
using Xunit;

namespace ReportIssueInPristina.Tests;

public class CurrentUserServiceTests
{
    [Fact]
    public async Task InitializeAsync_LoadsProfileAndAdminRole()
    {
        var factory = CreateContextFactory();
        await using (var db = factory.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser { Id = "user-1", Name = "Ada", Email = "ada@example.com" });
            await db.SaveChangesAsync();
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "test"));
        using var service = new CurrentUserService(
            new FixedAuthenticationStateProvider(principal),
            factory,
            NullLogger<CurrentUserService>.Instance);

        await service.InitializeAsync();

        service.UserId.Should().Be("user-1");
        service.Name.Should().Be("Ada");
        service.Email.Should().Be("ada@example.com");
        service.IsAuthenticated.Should().BeTrue();
        service.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_UsesSubjectClaimWhenNameIdentifierIsMissing()
    {
        var factory = CreateContextFactory();
        await using (var db = factory.CreateDbContext())
        {
            db.Users.Add(new ApplicationUser { Id = "subject-user", Name = "Sam" });
            await db.SaveChangesAsync();
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "subject-user")], "test"));
        using var service = new CurrentUserService(
            new FixedAuthenticationStateProvider(principal),
            factory,
            NullLogger<CurrentUserService>.Instance);

        await service.InitializeAsync();

        service.UserId.Should().Be("subject-user");
        service.IsAuthenticated.Should().BeTrue();
        service.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_ClearsStateWhenAuthenticatedUserIsNotPersisted()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "missing-user")], "test"));
        using var service = new CurrentUserService(
            new FixedAuthenticationStateProvider(principal),
            CreateContextFactory(),
            NullLogger<CurrentUserService>.Instance);

        await service.InitializeAsync();

        service.UserId.Should().BeNull();
        service.IsAuthenticated.Should().BeFalse();
        service.IsAdmin.Should().BeFalse();
    }

    private static TestDbContextFactory CreateContextFactory()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContextFactory(options);
    }

    private sealed class TestDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => new(options);
    }

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(user));
    }
}