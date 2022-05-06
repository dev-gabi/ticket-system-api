using Ganss.XSS;
using Services.Models.Auth;
using Services.Models.Customers;
using Services.Models.Employees;
using Services.Models.Tickets;
using System;
using System.Reflection;

namespace Services
{
    public interface ISanitizerService
    {
        string SanitizeString(string inputString);
        CustomerRegisterViewModel SanitizeCustomerRegisterViewModel(CustomerRegisterViewModel vm);
        EmployeeRegisterViewModel SanitizeEmployeeRegisterViewModel(EmployeeRegisterViewModel vm);
        LoginViewModel SanitizeLoginViewModel(LoginViewModel model);
        ResetPasswordViewModel SanitizeResetPasswordViewModel(ResetPasswordViewModel model);
        CreateTicketVM SanitizeCreateTicketViewModel(CreateTicketVM vm);
        CustomerVM SanitizeCustomerVM(CustomerVM updatedCustomer);
        EmployeePreRegisterViewModel SanitizeEmployeePreRegisterAsync(EmployeePreRegisterViewModel model);
        EmployeeEditVM SanitizeEmploeeEditVM(EmployeeEditVM vm);
        SupporterStatsVM SanitizeSupporterStatsVM(SupporterStatsVM vm);
    }
    public class SanitizerService : ISanitizerService
    {
        readonly HtmlSanitizer sanitizer;
        public SanitizerService()
        {
            sanitizer = new HtmlSanitizer();
        }
        public string SanitizeString(string inputString)
        {
            return sanitizer.Sanitize(inputString);
        }

        public CustomerRegisterViewModel SanitizeCustomerRegisterViewModel(CustomerRegisterViewModel vm)
        {
            vm.Address = SanitizeString(vm.Address);
            vm.ConfirmPassword = SanitizeString(vm.ConfirmPassword);
            vm.Email = SanitizeString(vm.Email);
            vm.Password = SanitizeString(vm.Password);
            vm.PhoneNumber = SanitizeString(vm.PhoneNumber);
            vm.UserName = SanitizeString(vm.UserName);
            return vm;
        }
        public EmployeeRegisterViewModel SanitizeEmployeeRegisterViewModel(EmployeeRegisterViewModel vm)
        {
            vm.ConfirmPassword = SanitizeString(vm.ConfirmPassword);
            vm.Email = SanitizeString(vm.Email);
            vm.Password = SanitizeString(vm.Password);
            vm.UserName = SanitizeString(vm.UserName);
            vm.PhoneNumber = SanitizeString(vm.PhoneNumber);
            return vm;
        }
        public LoginViewModel SanitizeLoginViewModel(LoginViewModel model)
        {
            model.Email = SanitizeString(model.Email);
            model.Password = SanitizeString(model.Password);
            return model;
        }
        public ResetPasswordViewModel SanitizeResetPasswordViewModel(ResetPasswordViewModel model)
        {
            model.ConfirmPassword = SanitizeString(model.ConfirmPassword);
            model.NewPassword = SanitizeString(model.NewPassword);
            model.ResetToken = SanitizeString(model.ResetToken);
            model.UserId = SanitizeString(model.UserId);
            return model;
        }
        public CreateTicketVM SanitizeCreateTicketViewModel(CreateTicketVM vm)
        {
            vm.Title = SanitizeString(vm.Title);
            vm.Message = SanitizeString(vm.Message);
            vm.Category = SanitizeString(vm.Category);
            return vm;
        }
        public CustomerVM SanitizeCustomerVM(CustomerVM updatedCustomer)
        {         
            updatedCustomer.Address = SanitizeString(updatedCustomer.Address);
            updatedCustomer.Email = SanitizeString(updatedCustomer.Email);
            updatedCustomer.Id = SanitizeString(updatedCustomer.Id);
            updatedCustomer.Name = SanitizeString(updatedCustomer.Name);
            updatedCustomer.PhoneNumber = SanitizeString(updatedCustomer.PhoneNumber);
            updatedCustomer.Role = SanitizeString(updatedCustomer.Role);
            return updatedCustomer;
        }

        public EmployeePreRegisterViewModel SanitizeEmployeePreRegisterAsync(EmployeePreRegisterViewModel model)
        {
            model.PersonalEmail = SanitizeString(model.PersonalEmail);
            model.Role = SanitizeString(model.Role);
            model.UserName = SanitizeString(model.UserName);
            return model;
        }

        public EmployeeEditVM SanitizeEmploeeEditVM(EmployeeEditVM vm)
        {
            vm.Email = SanitizeString(vm.Email);
            vm.Id = SanitizeString(vm.Id);
            vm.Name = SanitizeString(vm.Name);
            return vm;
        }

        public SupporterStatsVM SanitizeSupporterStatsVM(SupporterStatsVM vm)
        {
            vm.Id = SanitizeString(vm.Id);
            return vm;
        }
    }
}
