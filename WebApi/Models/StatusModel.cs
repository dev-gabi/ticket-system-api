using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class StatusModel
    {
        [Required, MaxLength(10)]
        public string Status { get; set; }
    }
}
