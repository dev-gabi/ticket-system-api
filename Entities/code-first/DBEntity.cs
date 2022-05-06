using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class DBEntity : IId
    {
        [Key]
        public int Id { get; set; }
    }
}
