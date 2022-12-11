using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class AuthLog:Log
    {
        [Required, MaxLength(6)]
        public string Action { get; set; }
    }
}
