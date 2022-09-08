using Dal;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.logs;
using Services.Models.Customers;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;
        private string error;
        public CustomersController(ISanitizerService sanitizer, GenericRepository<Customer> customersRepository, IErrorLogService errorLogService)
        {
            _customerService = new CustomerService(sanitizer, customersRepository, errorLogService, this.userId);
        }

        [HttpGet]
        [Route("api/customers/all")]
        public IActionResult GetAll()
        {
            var result = _customerService.GetAll(out error);

            return CreateHttpResponse(result, error);
        }
        [HttpGet]
        [Route("api/Customers/{id}")]
        public IActionResult GetById([FromRoute] string id)
        {
            var result =  _customerService.GetById(id, out error);

            return CreateHttpResponse(result, error);
        }
        [HttpPut]
        [Route("api/Customers")]
        public IActionResult Put([FromBody] CustomerVM customer)
        {
            if (ModelState.IsValid)
            {
                var result =  _customerService.Edit(customer, out error);

                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }
    }
}
