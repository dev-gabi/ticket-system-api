using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Models
{
    public class TypeAheadSearchModel
    {
        public string SearchInput { get; set; }
        #nullable enable
        public string? Role { get; set; }
        #nullable disable
    }
}
