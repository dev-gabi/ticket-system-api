using Dal;
using Entities;
using services.Enums;
using Services.logs;
using Services.Models.Customers;
using System;
using System.Threading.Tasks;
namespace Services
{
    public interface ICustomerService
    {
        Task<CustomerVM> GetById(string id);
        Task<CustomerVM> Edit(CustomerVM customer);
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

        public Task<CustomerVM> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _errorLogService.LogError("CustomerService - GetById: id is null", _userId);
                return null;
            }
            id = _sanitizer.SanitizeString(id);
            return Task.FromResult(
              _customersRepository.GetByID(id).ConvertToCustomerVM(Roles.Customer.ToString())                
                );
        }
        public Task<CustomerVM> Edit(CustomerVM updatedCustomer)
        {
            updatedCustomer = _sanitizer.SanitizeCustomerVM(updatedCustomer);
            try
            {
                Customer dbEntity = _customersRepository.GetByID(updatedCustomer.Id);
                
                _customersRepository.Update(UpdateEntityProps(dbEntity, updatedCustomer));
                _customersRepository.Save();

                return Task.FromResult(dbEntity.ConvertToCustomerVM(Roles.Customer.ToString()));
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"CustomerService - GetById: {x.Message} {x.InnerException}", _userId);
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
