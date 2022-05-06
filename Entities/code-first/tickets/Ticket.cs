using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entities
{
   public class Ticket: DBEntity
	{
        [Required]
		public string CustomerId { get; set; }
		[Required]
		public DateTime OpenDate { get; set; }
		public DateTime ClosingDate { get; set; }
		[Required, MaxLength(50,ErrorMessage ="Title shouldn't exceed over 50 chars")]
		public string Title { get; set; }
		public ICollection<Reply> Replies { get; set; }
		[Required]
		public string Status { get; set; }
		[Required, MaxLength(50, ErrorMessage = "Category shouldn't exceed over 50 chars")]
        public string Categoty { get; set; }
        public string ClosedByUser { get; set; }

    }
}
