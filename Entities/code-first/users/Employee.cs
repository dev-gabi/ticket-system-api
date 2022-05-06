using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
   public class Employee: User
    {
        [Required, EmailAddress]
        public string PersonalEmail { get; set; }
    }
}
