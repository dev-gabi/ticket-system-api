using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class DBEntity : IGenericEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
