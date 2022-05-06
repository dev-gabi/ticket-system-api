using System;

namespace Services.Models.Tickets
{
    public class AddReplyResponse 
    {
        public string Error { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool IsImageAttached { get; set; }
        public int ReplyId { get; set; }
        public string ImagePath { get; set; }
        public bool IsInnerReply { get; set; }
    }
}
