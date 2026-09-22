using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Web.Data;
using System.Security.Claims;

namespace ReportIssueInPristina.Web.Services
{
    public class CurrentUserService : ICurrentUserService, IDisposable
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<CurrentUserService> _logger;

        private Task? _initializationTask;

        public string? UserId { get; private set; }
        public string? Name { get; private set; }
        public string? Email { get; private set; }

        public bool IsAuthenticated { get; private set; }
        public bool IsAdmin { get; private set; }

        public CurrentUserService(
            AuthenticationStateProvider authenticationStateProvider,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            ILogger<CurrentUserService> logger)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _contextFactory = contextFactory;
            _logger = logger;

            _authenticationStateProvider.AuthenticationStateChanged
                += OnAuthenticationStateChanged;
        }

        public Task InitializeAsync()
        {
            return _initializationTask ??= InitializeCoreAsync();
        }

        private async Task InitializeCoreAsync()
        {

            var authenticationState =
                await _authenticationStateProvider.GetAuthenticationStateAsync();

            try
            {
                await UpdateUserAsync(authenticationState);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Loading the current user was canceled.");
            }
        }

        private async void OnAuthenticationStateChanged(
            Task<AuthenticationState> authenticationStateTask)
        {
            try
            {
                var authenticationState = await authenticationStateTask;
                await UpdateUserAsync(authenticationState);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Updating the current user was canceled.");
            }
        }

        private async Task UpdateUserAsync(
            AuthenticationState authenticationState)
        {
            var principal = authenticationState.User;

            Console.WriteLine(
                $"AUTH STATE: {principal.Identity?.IsAuthenticated}");

            if (principal.Identity?.IsAuthenticated != true)
            {
                ClearUser();
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                ClearUser();
                return;
            }

            await using var db = await _contextFactory.CreateDbContextAsync();
            var user = await db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId);

            if (user is null)
            {
                ClearUser();
                return;
            }

            UserId = user.Id;
            IsAuthenticated = true;
            IsAdmin = principal.IsInRole("Admin");
            Name = user.Name;
            Email = user.Email;

            Console.WriteLine(
                $"CURRENT USER: {Name} | {Email} | Admin: {IsAdmin}");
        }

        private void ClearUser()
        {
            UserId = null;
            Name = null;
            Email = null;

            IsAuthenticated = false;
            IsAdmin = false;
        }

        public void Dispose()
        {
            _authenticationStateProvider.AuthenticationStateChanged
                -= OnAuthenticationStateChanged;
        }
    }
}