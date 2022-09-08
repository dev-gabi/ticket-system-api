using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Employees
{
    public class EmployeeEditVM
    {
        [Required]
        public string Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
