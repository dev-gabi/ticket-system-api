using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
   public class Customer: User
    {
        //[Required , MaxLength(11, ErrorMessage = "phone number max length is 11 chars")]
        //public string PhoneNumber { get; set; }
        [MaxLength(200, ErrorMessage = "Adress max length is 200 chars")]
        public string Address { get; set; }
    }
}
