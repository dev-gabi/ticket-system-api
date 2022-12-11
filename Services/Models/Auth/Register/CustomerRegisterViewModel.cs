using Services;
using Services.Models.Auth;
using System.ComponentModel.DataAnnotations;


public class CustomerRegisterViewModel : RegisterViewModel
{
    [StringLength(200, MinimumLength = 4)]
    public string Address { get; set; }

}
