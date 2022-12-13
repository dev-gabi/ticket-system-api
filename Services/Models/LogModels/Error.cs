using Entities;
using System;

namespace Services.Models.Logs
{
    public class Error : DBEntity
    {
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public string ErrorDetails { get; set; }
    }
}
