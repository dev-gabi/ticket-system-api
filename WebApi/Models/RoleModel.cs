using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class RoleModel
    {
        [Required, MaxLength(20)]
        public string Role { get; set; }
    }
}
