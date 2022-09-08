using Dal;
using Entities;
using Microsoft.AspNetCore.Identity;
using Services.Models.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public static class ResponseHelpers
    {
        public static ApiResponse ApiResponseError(List<string> errors)
        {
            string error = "";
            errors.ForEach(e => error += e + ", ");
            return new RegisterManagerResponse
            {
                Message = "Error",
                StatusCode = 400,
                StatusCodeTitle = "Bad Request",
                IsSuccess = false,
                Errors = error
            };
        }

        public static ApiResponse ApiResponseError(string errorMessage)
        {
            return new RegisterManagerResponse
            {
                Message = "Error",
                StatusCode = 400,
                StatusCodeTitle = "Bad Request",
                IsSuccess = false,
                Errors = errorMessage
            };
        }

        public static ApiResponse ApiResponseSuccess(string message)
        {
            return new ApiResponse
            {
                Message = message,
                IsSuccess = true,
                StatusCode = 200,
                StatusCodeTitle = "ok"
            };
        }

        public static ApiResponse ApiResponseNotFound()
        {
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Error",
                StatusCode = 404,
                StatusCodeTitle = "Not found",
                Errors = "User not found"
            };
        }

        public static async Task<ApiResponse> ValidateRegisterModel(RegisterViewModel model, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager)
        {
            if (model == null)
                throw new NullReferenceException("Reigster Model is null");

            if (model.Password != model.ConfirmPassword)
            {
                return ApiResponseError("Confirm password doesn't match the password") as RegisterManagerResponse;
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                return ApiResponseError($"Email {model.Email} was already registered") as RegisterManagerResponse;
            }
            if (!ExtensionMethods.IsValidEmail(model.Email))
            {
                return ApiResponseError("Invalid Email") as RegisterManagerResponse;
            }
            return null;
        }


        public static  ApiResponse ValidatePreRegisterModel(EmployeePreRegisterViewModel model, GenericRepository<Employee> employeeRepository)
        {
            if (model == null)
                throw new NullReferenceException("Reigster Model is null");
           if (employeeRepository.GetOne(e => e.UserName == model.UserName) != null)
                return ApiResponseError("User name already exists");

            if (employeeRepository.GetOne(e => e.PersonalEmail == model.PersonalEmail) != null)
                return ApiResponseError("Personal email already exists ");

            return null;
        }
        public static LoginManagerResponse ValidateLoginModel(LoginViewModel model, UserManager<IdentityUser> _userManager, User user)
        {          
            if (user == null)
            {
                return ApiResponseNotFound().ConvertToLoginManagerResponse();
            }
            if (!user.IsActive)
            {
                return ApiResponseError("User is not active. please consult your admin").ConvertToLoginManagerResponse();
            }

            var result =  _userManager.CheckPasswordAsync(user, model.Password).Result;

            if (!result)
            {
                return ApiResponseError("Invalid password").ConvertToLoginManagerResponse();
            }

            return null;
        }
    }
}
