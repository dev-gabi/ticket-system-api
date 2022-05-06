using Services.Models.Auth;
using System.ComponentModel.DataAnnotations;


public class EmployeeRegisterViewModel : RegisterViewModel
    {
        //[Required]
        //[StringLength(20)]
        //public string Role { get; set; }
        //[Required]
        //[StringLength(100)]
        //[EmailAddress]
        //public string PersonalEmail { get; set; }

        [Required]
        public string ResetToken { get; set; }
}

