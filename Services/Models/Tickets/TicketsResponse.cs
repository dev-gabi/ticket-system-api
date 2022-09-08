using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Tickets
{
    public class TicketsResponse
    {
        public TicketResponse[] Tickets { get; set; }
        public string Error { get; set; }
    }
}
