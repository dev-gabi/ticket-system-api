using Services.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Services.Models.Auth
{
    public class EmployeeRegisterViewModel : RegisterViewModel
    {

        [Required]
        public string ResetToken { get; set; }

    }

}