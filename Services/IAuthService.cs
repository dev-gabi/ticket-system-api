using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using services.Enums;
using Services.Helpers;
using Services.logs;
using Services.Models.Auth;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public interface IAuthService
    {
        Task<ApiResponse> CustomerRegisterAsync(CustomerRegisterViewModel model);
        Task<ApiResponse> EmployeePreRegisterAsync(EmployeePreRegisterViewModel model);
        Task<ApiResponse> EmployeeRegisterAsync(EmployeeRegisterViewModel model);
        Task<LoginManagerResponse> LoginUserAsync(LoginViewModel model);
        Task<ApiResponse> ConfirmEmailAsync(string userId, string token);
        Task<bool> SendConfirmationEmailAsync(IdentityUser identityUser = null);
        Task<ApiResponse> ForgetPasswordAsync(string email);
        Task<ApiResponse> ReSendConfirmationEmailAsync(string email);
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<bool> ValidateRegistrationTokenAsync(string token, string email);
        Task<ApiResponse> ResendEmailWithFreshTokenEmployeePreRegisterAsync(string employeePersonalEmail);
        void Logout(string userId, Microsoft.AspNetCore.Http.HttpContext context);
    }

    public class AuthService : IAuthService
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        internal GenericRepository<Employee> EmployeeRepository;
        private readonly IAuthLogService _authLogService;
        private readonly IEmailService _emailService;
        private readonly ISanitizerService _sanitizer;
        private readonly IErrorLogService _errorLogService;
        private readonly JwtConfig _jwtConfig;
        private readonly DomainConfig _domainConfig;
        private readonly EmployeesSettingsConfig _employeesSettingsConfig;
        private readonly string _userId;
        private const string anonymousMethod = "Anonymous method";

        public AuthService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, GenericRepository<Employee> employeeRepository,
     IEmailService emailService, ISanitizerService sanitizer, IErrorLogService errorLogService, IAuthLogService authLogService, 
    IOptions<JwtConfig> jwtConfig, IOptions<DomainConfig> domainConfig,
     IOptions<EmployeesSettingsConfig> employeesSettingConfig, string userId)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            EmployeeRepository = employeeRepository;
            _emailService = emailService;
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _authLogService = authLogService;
            _jwtConfig = jwtConfig.Value;
            _domainConfig = domainConfig.Value;
            _employeesSettingsConfig = employeesSettingConfig.Value;
            _userId = userId;

        }

        public async Task<ApiResponse> CustomerRegisterAsync(CustomerRegisterViewModel model)
        {
            model = _sanitizer.SanitizeCustomerRegisterViewModel(model);
            ApiResponse responseWithError = ResponseHelpers.ValidateRegisterModel(model, _userManager).Result;
            if (responseWithError != null) return responseWithError as RegisterManagerResponse;

            try
            {
                IdentityUser identityUser = model.ConvertToIdentityUser();
                var result = await _userManager.CreateAsync(identityUser, model.Password);

                if (result.Succeeded)
                {
                    await RegisterRole(Roles.Customer.ToString(), identityUser);
                    await SendConfirmationEmailAsync(identityUser);
                    return ResponseHelpers.ApiResponseSuccess("A confirmation link was sent to your email.");
                }
                return ResponseHelpers.ApiResponseError(result.Errors.Select(e => e.Description).ToList()) as RegisterManagerResponse;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - CustomerRegisterAsync: {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An Error Occured. please contact your web master") as RegisterManagerResponse;
            }
        }
        /// <summary>
        /// Register a new user by creating a corporate email and send a link to his personal email to set a new password.
        /// </summary>
        /// <param name="model">EmployeePreRegisterViewModel</param>
        /// <returns>Task<ApiResponse></returns>
        public async Task<ApiResponse> EmployeePreRegisterAsync(EmployeePreRegisterViewModel model)
        {
            model = _sanitizer.SanitizeEmployeePreRegisterAsync(model);
            ApiResponse responseWithError = ResponseHelpers.ValidatePreRegisterModel(model, EmployeeRepository);
            if (responseWithError != null) return responseWithError;

            if (!Enum.IsDefined(typeof(Roles), model.Role))
            {
                _errorLogService.LogError($"AuthSerivce - EmployeePreRegisterAsync: Invalid Role: {model.Role}", _userId);
                return ResponseHelpers.ApiResponseError("Invalid role");
            }
            try
            {
                IdentityUser user = model.ConvertToIdentityUser();
                user.Email = AuthHelpers.GenerateCorporateEmail(user.UserName, _employeesSettingsConfig.EmailSuffix);
                IdentityResult registerResult = null;
                registerResult = await _userManager.CreateAsync(user, AuthHelpers.GenerateRandomPassword());
                await RegisterRole(model.Role, user);

                ApiResponse success = await CreateResetPasswordTokenAndSendMailToEmployeePersonalEmail(user, model.PersonalEmail);
                if (success != null) return success;
                return ResponseHelpers.ApiResponseError(registerResult.Errors.Select(e => e.Description).ToList());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthSerivce - EmployeePreRegisterAsync:  {x.Message} {x.InnerException}", _userId);
                return ResponseHelpers.ApiResponseError("An Error Occured. please contact your web master");
            }
        }
        /// <summary>
        /// Reset the password according to user's input and update phone number
        /// </summary>
        /// <param name="model">EmployeeRegisterViewModel</param>
        /// <returns>Task<ApiResponse></returns>
        public async Task<ApiResponse> EmployeeRegisterAsync(EmployeeRegisterViewModel model)
        {
            model = _sanitizer.SanitizeEmployeeRegisterViewModel(model);
            if (model == null)
            {
                _errorLogService.LogError($"AuthService - EmployeeRegisterAsync: model is null", anonymousMethod);
                return ResponseHelpers.ApiResponseError("model is null");
            }
            if (model.Password != model.ConfirmPassword) { return ResponseHelpers.ApiResponseError("Confirm password doesn't match the password"); }
            try
            {
                IdentityUser user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) { return ResponseHelpers.ApiResponseNotFound(); }

                IdentityResult resetResult = await _userManager.ResetPasswordAsync(user, AuthHelpers.DecodeToken(model.ResetToken), model.Password);

                if (resetResult.Succeeded)
                {
                    string personalEmail = UpdatePhoneNumberAndEmailConfirmedStatus(user.Id, model.PhoneNumber);
                    Task<bool> sendSuccessEmail = _emailService.Send(user.UserName, personalEmail, "Success Registration To Ticket-System",
                        "<h1>Registration Completed</h1>" +
                        $"<p>Hi, {user.UserName} <br/> You have completed your registration to our system." +
                        $"<br/> Your corporate email is: {user.Email}<br/> you can log in by <a href='{_domainConfig.Login}'>here</a></p>");
                    return ResponseHelpers.ApiResponseSuccess("User Registered Successfully!");
                }
                return ResponseHelpers.ApiResponseError(resetResult.Errors.Select(e => e.Description).ToList());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - EmployeeRegisterAsync:  {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An Error Occured. please contact your web master") as RegisterManagerResponse;
            }
        }

        /// <summary>
        /// updates phone number and email confirmation status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="phoneNumber"></param>
        /// <returns>user's personal email</returns>
        private string UpdatePhoneNumberAndEmailConfirmedStatus(string id, string phoneNumber)
        {
            Employee e = EmployeeRepository.GetByID(id);
            e.EmailConfirmed = true;
            e.IsActive = true;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                e.PhoneNumber = phoneNumber;
            }
            EmployeeRepository.Update(e);
            EmployeeRepository.Save();
            return e.PersonalEmail;
        }

        public async Task<ApiResponse> ResendEmailWithFreshTokenEmployeePreRegisterAsync(string employeePersonalEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(employeePersonalEmail)) { return ResponseHelpers.ApiResponseError("email is null"); }
                var employee = EmployeeRepository.GetOne(e => e.PersonalEmail == employeePersonalEmail);
                if(employee == null) { return ResponseHelpers.ApiResponseNotFound(); }  

                IdentityUser user = await _userManager.FindByIdAsync(employee.Id);
                Task<ApiResponse> success = CreateResetPasswordTokenAndSendMailToEmployeePersonalEmail(user, employeePersonalEmail);
                if (success != null) return success.Result;
                return ResponseHelpers.ApiResponseError("An error had occured while trying to send an email.");
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - ResendEmailWithFreshTokenEmployeePreRegisterAsync:  {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An Error Occured. please contact your web master") as RegisterManagerResponse;
            }
        }

        async Task<ApiResponse> CreateResetPasswordTokenAndSendMailToEmployeePersonalEmail(IdentityUser user, string personalEmail)
        {

            string token = await GenerateEncodedResetPasswordToken(user);
            string url = $"{_domainConfig.EmployeeRegister}?token={token}&userName={user.UserName}&email={user.Email}";

            bool isEmailSent = await _emailService.Send(user.UserName, personalEmail, "Your registration link to ticket system", "<h1>Ticket system employee registration link</h1>" +
                $"<p><a href='{url}'>Click here</a> to register</p>" +
                "<p>This link will not be valid in 1 hour.</p>", _userId);
            if (isEmailSent)
            {
                return ResponseHelpers.ApiResponseSuccess("Registration link has been sent to employee personal email");
            }
            return null;
        }

        public async Task<LoginManagerResponse> LoginUserAsync(LoginViewModel model)
        {
            model = _sanitizer.SanitizeLoginViewModel(model);
            IdentityUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user==null) { return ResponseHelpers.ApiResponseError("Email is not registered").ConvertToLoginManagerResponse(); }
            if (!user.EmailConfirmed) { return ResponseHelpers.ApiResponseError("Please confirm your email to log in").ConvertToLoginManagerResponse(); }
            LoginManagerResponse responseWithError = ResponseHelpers.ValidateLoginModel(model, _userManager, user as User);
            if (responseWithError != null) return responseWithError;
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.EncKey));
                int expirationDurationInSeconds = int.Parse(_jwtConfig.TokenExpirationDurationInSeconds);
                DateTime tokenExpiration = DateTime.UtcNow.AddSeconds(expirationDurationInSeconds);//JwtSecurityToken uses UTC time - use DateTime.UtcNow to set expire time

                var token = new JwtSecurityToken(
                    issuer: _jwtConfig.Issuer,
                    audience: _jwtConfig.Audience,
                    claims: AuthHelpers.SetUserClaims(user, _userManager),
                    expires: tokenExpiration,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
                _authLogService.LogLogin(user.Id, "Success");
                return UserLoggedInSuccessfully(user, token, expirationDurationInSeconds);
            }
            catch (Exception x)
            {
                _authLogService.LogLogin(user.Id, "Failure");
                _errorLogService.LogError($"AuthSerivce - LoginUserAsync:  {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An Error Occured. please contact your web master").ConvertToLoginManagerResponse();
            }
        }

        public async Task RegisterRole(string roleName, IdentityUser identityUser)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole();
                role.Name = roleName;
                await _roleManager.CreateAsync(role);
            }
            await _userManager.AddToRoleAsync(identityUser, roleName);
        }

        private LoginManagerResponse UserLoggedInSuccessfully(IdentityUser user, JwtSecurityToken token, int expirationDurationInSeconds)
        {
            string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginManagerResponse
            {
                Token = tokenAsString,
                Message = "Logged in successfully",
                IsSuccess = true,
                ExpireInSeconds = expirationDurationInSeconds.ToString(),
                StatusCode = 200,
                Id = user.Id,
                StatusCodeTitle = "ok",
                UserName = user.UserName,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
            };
        }

        public async Task<ApiResponse> ConfirmEmailAsync(string userId, string token)
        {
            userId = _sanitizer.SanitizeString(userId);
            token = _sanitizer.SanitizeString(token);
            if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException("userId is null"); }
            if (string.IsNullOrEmpty(token)) { throw new ArgumentNullException("token is null"); }
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return ResponseHelpers.ApiResponseNotFound();

                if (user.EmailConfirmed) { return ResponseHelpers.ApiResponseSuccess("Email was already confirmed"); }
                var result = await _userManager.ConfirmEmailAsync(user, AuthHelpers.DecodeToken(token));

                if (result.Succeeded)
                {
                    return ResponseHelpers.ApiResponseSuccess("Email confirmed successfully");
                }                   

                return ResponseHelpers.ApiResponseError(result.Errors.Select(e => e.Description).ToList());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - ConfirmEmailAsync:  {x.Message} {x.InnerException}", userId);
                return ResponseHelpers.ApiResponseError("An unknown error has occured");
            }
        }

        public async Task<bool> SendConfirmationEmailAsync(IdentityUser identityUser)
        {
            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
            var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

            string url = $"{_domainConfig.BackEnd}/api/auth/confirm-email?userid={identityUser.Id}&token={validEmailToken}";
            return await _emailService.Send(identityUser.UserName, identityUser.Email, "Ticket System Registration Confirmation", $"<h1>Welcome to Support Ticket System</h1>" +
               $"<p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");
        }

        public async Task<ApiResponse> ReSendConfirmationEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) { throw new ArgumentNullException("email is null"); }
            email = _sanitizer.SanitizeString(email);
            try
            {
                IdentityUser identityUser = await _userManager.FindByEmailAsync(email);
                if (identityUser == null) { return ResponseHelpers.ApiResponseNotFound(); }

                if (await SendConfirmationEmailAsync(identityUser))
                {
                    return ResponseHelpers.ApiResponseSuccess("Email was sent");
                }
                return ResponseHelpers.ApiResponseError("There was a problem while trying to send an email confirmation");
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - ReSendConfirmationEmailAsync for adress {email}:  {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An error has occured");
            }
        }

        public async Task<ApiResponse> ForgetPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) { throw new ArgumentNullException("email is null"); }
            email = _sanitizer.SanitizeString(email);
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return ResponseHelpers.ApiResponseNotFound();

                string validToken = await GenerateEncodedResetPasswordToken(user);

                string url = $"{_domainConfig.ForgotPassword}/?token={validToken}&id={user.Id}&email={user.Email}";

                await _emailService.Send(user.UserName, user.Email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
                    $"<p>To reset your password <a href='{url}'>Click here</a></p>" +
                    "<p>This link will not be valid in 1 hour.</p>");

                return ResponseHelpers.ApiResponseSuccess("Please check your email to reset your password");
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService - ForgetPasswordAsync for adress {email}:  {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("Am error has occured");
            }

        }

        private async Task<string> GenerateEncodedResetPasswordToken(IdentityUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            return WebEncoders.Base64UrlEncode(encodedToken);
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            model = _sanitizer.SanitizeResetPasswordViewModel(model);
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return ResponseHelpers.ApiResponseNotFound();
            try
            {
                if (model.NewPassword != model.ConfirmPassword)
                    return ResponseHelpers.ApiResponseError("Password doesn't match its confirmation");

                IdentityResult resetResult = await _userManager.ResetPasswordAsync(user, AuthHelpers.DecodeToken(model.ResetToken), model.NewPassword);
                if (resetResult.Succeeded)
                    return ResponseHelpers.ApiResponseSuccess("Password has been reset successfully!");

                return ResponseHelpers.ApiResponseError(resetResult.Errors.Select(e => e.Description).ToList());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService -  ResetPasswordAsync: {x.Message} {x.InnerException}", anonymousMethod);
                return ResponseHelpers.ApiResponseError("An error has occured while trying to reset password");
            }

        }

        public async Task<bool> ValidateRegistrationTokenAsync(string token, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) { throw new ArgumentNullException("token is null"); }
                if (string.IsNullOrEmpty(email)) { throw new ArgumentNullException("email is null"); }
                IdentityUser user = await _userManager.FindByEmailAsync(email);
                if (user == null) { return false; }

                if (await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", AuthHelpers.DecodeToken(token)))
                {
                    return true;//token is valid
                }
                else
                {
                    return false;//token is invalid
                }
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService -  ValidateRegistrationTokenAsync. provided email is {email}: {x.Message} {x.InnerException}", anonymousMethod);
                return false;
            }

        }

        public void Logout(string userId, HttpContext context)
        {
            userId = _sanitizer.SanitizeString(userId);
            try
            {
                context.Items.Remove("Id");
                context.Items.Remove("UserName");
                context.Items.Remove("Role");
                context.Request.Headers.Remove("Authorization");
                _authLogService.LogLogout(userId, "Success");
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthService -  RecordLogout: {x.Message} {x.InnerException}", _userId);
            }
        }


    }
}
