using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public abstract class User : IdentityUser, IId
    {

        //[Required, MaxLength(100, ErrorMessage = "max length is 100 chars")]
        //public string Role { get; set; }
        //[Required, EmailAddress(ErrorMessage = "Invalid Email"), MaxLength(100, ErrorMessage = "max length is 100 chars")]
        //public string Email { get; set; }
        [Required]
        public DateTime RegistrationDate { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
