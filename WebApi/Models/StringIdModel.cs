﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Models
{
    public class StringIdModel
    {
        [Required]
        public string Id { get; set; }
    }
}
