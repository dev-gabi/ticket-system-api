using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Employees
{
    public class SupporterStats
    {
        public DateTime Date { get; set; }
        public int TicketsClosed { get; set; }
        public int Replies { get; set; }
    }
}
