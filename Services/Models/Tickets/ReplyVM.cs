using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Tickets
{
    public class ReplyVM
    {
        [Required]
        public int  TicketId { get; set; }
        [Required]
        public string Message { get; set; }
#nullable enable
        public IFormFile? Image { get; set; }
#nullable disable
        public bool IsInnerReply { get; set; }
    }
}
