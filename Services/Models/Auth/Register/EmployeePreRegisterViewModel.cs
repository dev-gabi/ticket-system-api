﻿using System.ComponentModel.DataAnnotations;

namespace Services.Models.Auth
{
    public class EmployeePreRegisterViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 2,ErrorMessage =("User name should be at least 2 characters"))]
        public string UserName { get; set; }
        [Required]
        [StringLength(20)]
        public string Role { get; set; }
        [Required]
        [StringLength(100)]
        [EmailAddress(ErrorMessage ="Email is not valid")]
        public string PersonalEmail { get; set; }

     

    }
}
