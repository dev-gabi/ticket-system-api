using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Employees
{
    public class EmployeeResponse 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public IEnumerable<SupporterStats> Stats { get; set; }
       // public string Error { get; set; }
    }
}
