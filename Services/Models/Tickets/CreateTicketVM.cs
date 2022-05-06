using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Services.Models.Tickets
{
    public class CreateTicketVM
    {
        [Required, MaxLength(30, ErrorMessage ="Max 30 characters for the title"), MinLength(4, ErrorMessage ="Title should be at least 4 characters long")]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        [Required, MaxLength(50, ErrorMessage = "Category shouldn't exceed over 50 chars")]
        public string Category { get; set; }
            #nullable enable
        public IFormFile? Image { get; set; }
        #nullable disable
    }
}
