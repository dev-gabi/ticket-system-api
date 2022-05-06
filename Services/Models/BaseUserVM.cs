using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class BaseUserVM
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Boolean IsActive { get; set; }
    }
}
