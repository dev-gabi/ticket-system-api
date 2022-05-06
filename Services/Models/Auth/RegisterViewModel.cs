using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z]).{6,30}$", ErrorMessage = "Password must contain at least one digit and one letter")]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z]).{6,30}$", ErrorMessage = "Password confirmation must contain at least one digit and one letter")]
        [StringLength(50, MinimumLength = 6)]
        public string ConfirmPassword { get; set; }

        [StringLength(10)]
        public string PhoneNumber { get; set; }
    
    }
}
