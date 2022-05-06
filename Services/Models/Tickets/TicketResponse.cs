using Entities;
using System;

namespace Services.Models.Tickets
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public Reply[] Replies { get; set; }
        public string Status { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public string ClosedByUser { get; set; }
    }
}
