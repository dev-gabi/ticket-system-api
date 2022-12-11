using System.ComponentModel.DataAnnotations;

namespace Services.Models.Auth
{
    public class RegisterViewModel 
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string UserName { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z]).{6,30}$", ErrorMessage = "Password must contain at least one digit and one letter")]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }

        [StringLength(10)]
        public string PhoneNumber { get; set; }

    }
}
