using System;
using System.Collections.Generic;
using System.Text;

namespace ReportIssueInPristina.Domain.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public string? Title {  get; set; }
        public string? Description { get; set; }
        public string? Location {  get; set; }
        public string? ImageUrl {  get; set; }
        public string? Status {  get; set; }
        public DateTime DateCreated {  get; set; } = DateTime.Now;
        //Foreign key
        public int CategoryId {  get; set; }
        //Navigation property
        public virtual Category Category { get; set; }
        //Foreign Key
        public string? ApplicationUserId {  get; set; }
    }
}
