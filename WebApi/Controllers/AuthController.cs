using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using services.Enums;
using Services;
using Services.logs;
using Services.Models.Auth;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private IAuthService _authService;
        private readonly DomainConfig _domainConfig;

        public AuthController( UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            GenericRepository<Employee> employeeRepository, IEmailService emailService, ISanitizerService sanitizer,
            IErrorLogService errorLogService, IAuthLogService authLogSerice,
            IOptions<JwtConfig> jwtConfig, IOptions<EmployeesSettingsConfig> employeesSettingConfig, IOptions<DomainConfig> domainConfig)
        {
            _authService = new AuthService(userManager, roleManager, employeeRepository, emailService, sanitizer, errorLogService, authLogSerice, 
                jwtConfig, domainConfig, employeesSettingConfig, userId);
            _domainConfig = domainConfig.Value;
        }

        [HttpPost("customer-register")]
        public async Task<IActionResult> CustomerRegisterAsync([Bind("UserName, Email, Password, ConfirmPassword, PhoneNumber, Address")][FromBody] CustomerRegisterViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _authService.CustomerRegisterAsync(model);
                result ??= new RegisterManagerResponse() { IsSuccess = false, Errors = "server Error, contact your webmaster." };
                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("An error occured while trying to register a customer");
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpPost("employee-preregister")]
        public async Task<IActionResult> EmployeePreRegisterAsync([Bind("UserName, PersonalEmail, Role")][FromBody] EmployeePreRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.EmployeePreRegisterAsync(model);
                result ??= new ApiResponse() { IsSuccess = false, Errors = "server Error, contact your webmaster." };
                return Ok(result);
            }
            return BadRequest("An error occured while trying to register an employee");
        }

        [HttpPost("employee-register")]
        public async Task<IActionResult> EmployeeRegisterAsync([Bind("Email, Password, ConfirmPassword, Token, PhoneNumber")][FromBody] EmployeeRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.EmployeeRegisterAsync(model);
                result ??= new ApiResponse() { IsSuccess = false, Errors = "server Error, contact your webmaster." };
                return Ok(result);
            }
            return BadRequest("An error occured while trying to register an employee");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([Bind("Email, Password")] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.LoginUserAsync(model);
                result ??= new LoginManagerResponse() { IsSuccess = false, Errors = "server Error, contact your webmaster." };

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }
            return BadRequest("An error occured while trying to log in");
        }

        [HttpGet("logout")]
        public void Logout()
        {
            this._authService.Logout(userId, context);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            if (result.IsSuccess)
            {
                return Redirect(_domainConfig.RegistrationResult);
            }
            else
            {
                return Redirect($"{_domainConfig.RegistrationResult}?error={result.Errors}");
            }
        }

        [HttpPost("reconfirm-email")]
        public async Task<IActionResult> ReSendConfirmEmailAsync(EmailModel model)
        {
            var result = await _authService.ReSendConfirmationEmailAsync(model.Email);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync(EmailModel model)
        {
            var result = await _authService.ForgetPasswordAsync(model.Email);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var result = await _authService.ResetPasswordAsync(model);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpPost("refresh-registration-token")]
        public async Task<IActionResult> RefreshTokenForPreRegisteredEmployee(EmailModel model)
        {
            var result = await _authService.ResendEmailWithFreshTokenEmployeePreRegisterAsync(model.Email);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("validate-registration-token")]
        public async Task<bool> ValidateRegistrationToken(ValidateTokenModel model)
        {
            return await _authService.ValidateRegistrationTokenAsync(model.Token, model.Email);
        }
    }

}
