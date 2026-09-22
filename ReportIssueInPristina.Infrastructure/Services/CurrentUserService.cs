using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ReportIssueInPristina.Application.Services;
using ReportIssueInPristina.Web.Data;

namespace ReportIssueInPristina.Web.Services
{
    public class CurrentUserService : ICurrentUserService, IDisposable
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserManager<ApplicationUser> _userManager;

        private bool _initialized;

        public string? UserId { get; private set; }
        public string? Name { get; private set; }
        public string? Email { get; private set; }

        public bool IsAuthenticated { get; private set; }
        public bool IsAdmin { get; private set; }

        public CurrentUserService(
            AuthenticationStateProvider authenticationStateProvider,
            UserManager<ApplicationUser> userManager)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _userManager = userManager;

            _authenticationStateProvider.AuthenticationStateChanged
                += OnAuthenticationStateChanged;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            _initialized = true;

            var authenticationState =
                await _authenticationStateProvider.GetAuthenticationStateAsync();

            await UpdateUserAsync(authenticationState);
        }

        private async void OnAuthenticationStateChanged(
            Task<AuthenticationState> authenticationStateTask)
        {
            var authenticationState = await authenticationStateTask;

            await UpdateUserAsync(authenticationState);
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

            var user = await _userManager.GetUserAsync(principal);

            if (user == null)
            {
                ClearUser();
                return;
            }

            UserId = user.Id;
            Name = user.Name;
            Email = user.Email;

            IsAuthenticated = true;
            IsAdmin = principal.IsInRole("Admin");

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