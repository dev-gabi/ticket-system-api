using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class StatusModel
    {
        [Required, MaxLength(10)]
        public string Status { get; set; }
    }
}
