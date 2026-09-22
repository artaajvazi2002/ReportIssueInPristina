namespace ReportIssueInPristina.Application.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Name { get; }
        string? Email { get; }

        bool IsAuthenticated { get; }
        bool IsAdmin { get; }

        Task InitializeAsync();
    }
}