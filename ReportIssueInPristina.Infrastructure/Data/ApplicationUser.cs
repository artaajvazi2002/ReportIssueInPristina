using Microsoft.AspNetCore.Identity;
using ReportIssueInPristina.Domain.Models;

namespace ReportIssueInPristina.Web.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Name {  get; set; }
        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }

}
