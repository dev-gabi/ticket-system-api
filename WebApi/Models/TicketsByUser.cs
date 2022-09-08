using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class TicketsByUser
    {
        [Required]
        public string Id { get; set; }
        [Required, MaxLength(6,ErrorMessage ="Status length shoud be 6 chars max")]
        public string Status { get; set; }
    }
}
