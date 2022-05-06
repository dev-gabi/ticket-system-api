using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
   public class Reply: DBEntity
	{
		[Required]
		public int TicketId { get; set; }
		[Required]
		public string UserId { get; set; }
		[Required]
		public string UserName { get; set; }
		[Required]
		public string Message { get; set; }
        public DateTime Date { get; set; }
		public bool IsImageAttached { get; set; }
#nullable enable
		public ReplyImage? Image { get; set; }
#nullable disable
		public bool IsInnerReply { get; set; }
		//	public virtual Ticket Ticket { get; set; }
	}
}
