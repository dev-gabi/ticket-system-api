using Dal;
using Entities;
using services.Enums;
using Services.logs;
using Services.Models;
using Services.Models.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Services
{
    public interface ICustomerService
    {
        IEnumerable<CustomerVM> GetAll(out string error);
        CustomerVM GetById(string id, out string error);
        CustomerVM Edit(CustomerVM customer, out string error);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ISanitizerService _sanitizer;
        private readonly IErrorLogService _errorLogService;
        internal GenericRepository<Customer> _customersRepository;
        private readonly string _userId;


        public CustomerService(ISanitizerService sanitizer, GenericRepository<Customer> customersRepository, IErrorLogService errorLogService, string userId)
        {
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _customersRepository = customersRepository;
            _userId = userId;
        }

        public IEnumerable<CustomerVM> GetAll(out string error)
        {
            try
            {
                error = string.Empty;
                return _customersRepository.Get().ToList().ConvertToCustomerVMList(Roles.Customer.ToString());
            }
            catch (Exception x)
            {
            _errorLogService.LogError($"CustomerService - GetAll: {x.Message} {x.InnerException}", _userId);
            error = "An error occured while getting all customers";
            return null;
            }
        }
        public CustomerVM GetById(string id, out string error)
        {
            if (string.IsNullOrEmpty(id))
            {
                _errorLogService.LogError("CustomerService - GetById: id is null", _userId);
                error = $"customer with id: {id} doesn't exist";
                return null;
            }
            try
            {
                id = _sanitizer.SanitizeString(id);
                error = string.Empty;
                return _customersRepository.GetByID(id).ConvertToCustomerVM(Roles.Customer.ToString());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"CustomerService - GetAll: {x.Message} {x.InnerException}", _userId);
                error = $"An error occured while getting customer by id {id}";
                return null;
            }
                        
        }

        public CustomerVM Edit(CustomerVM updatedCustomer, out string error)
        {
            updatedCustomer = _sanitizer.SanitizeCustomerVM(updatedCustomer);
            try
            {
                Customer dbEntity = _customersRepository.GetByID(updatedCustomer.Id);
                
                _customersRepository.Update(UpdateEntityProps(dbEntity, updatedCustomer));
                _customersRepository.Save();
                error = string.Empty;
                return dbEntity.ConvertToCustomerVM(Roles.Customer.ToString());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"CustomerService - GetById: {x.Message} {x.InnerException}", _userId);
                error = $"An error occured while updating customer {updatedCustomer.Id}";
                return null;
            }
        }

        private Customer UpdateEntityProps(Customer dbEntity, CustomerVM updatedCustomer)
        {
            dbEntity.UserName = updatedCustomer.Name;
            dbEntity.NormalizedUserName = updatedCustomer.Name.ToUpper();
            dbEntity.Address = updatedCustomer.Address;
            dbEntity.Email = updatedCustomer.Email;
            dbEntity.NormalizedEmail = updatedCustomer.Email.ToUpper();
            dbEntity.PhoneNumber = updatedCustomer.PhoneNumber;

            return dbEntity;
        }
    }
}
