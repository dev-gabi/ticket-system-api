using System;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public abstract class Log : DBEntity
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Details { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
