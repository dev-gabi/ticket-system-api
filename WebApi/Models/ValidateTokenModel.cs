using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class ValidateTokenModel
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
