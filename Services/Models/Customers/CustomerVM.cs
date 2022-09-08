using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Customers
{
   public class CustomerVM 
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Role { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [MinLength(10, ErrorMessage ="phone number should be 10 digits"), MaxLength(10,ErrorMessage = "phone number should be 10 digits")]
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
    }
}
