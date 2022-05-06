using System;
using System.Collections.Generic;

namespace Services.Models.Employees
{
    public class SupporterStatsResponse
    {
        public List<SupporterStats> SupporterStats { get; set; }
        public string Error { get; set; }
    }
}
