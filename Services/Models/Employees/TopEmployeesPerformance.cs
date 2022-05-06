using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Employees
{
    public class TopEmployeesPerformance
    {
        public IEnumerable<TopEmployeesPerformanceByName> EmployeeStats { get; set; }
        public int MaxValue { get; set; }
    }
    public class TopEmployeesPerformanceByName
    {
        public int TicketsClosed { get; set; }
        public string UserName { get; set; }
    }
    public class TopEmployeesPerformanceById
    {
        public int TicketsClosed { get; set; }
        public string Id { get; set; }
    }
}
