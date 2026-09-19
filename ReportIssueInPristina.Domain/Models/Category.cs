using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace ReportIssueInPristina.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    }
}
