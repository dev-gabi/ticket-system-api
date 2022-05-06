using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Employees
{
    public class GeneralMonthlyStats
    {
        public int TotalClosedTickets { get; set; }
        public int ClosedTicketsThatWereOpenThisMonth { get; set; }
        public int TotalReplies { get; set; }
        public int OpenedTickets { get; set; }
    }
}
