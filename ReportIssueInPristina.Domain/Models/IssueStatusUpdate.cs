namespace ReportIssueInPristina.Domain.Models
{
    public class IssueStatusUpdate
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public string? PreviousStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string ChangedByUserId { get; set; } = string.Empty;
        public string ChangedByName { get; set; } = string.Empty;
        public DateTimeOffset ChangedAt { get; set; }
        public string? Note { get; set; }
        public Issue? Issue { get; set; }
    }
}