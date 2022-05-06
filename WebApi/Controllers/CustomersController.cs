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
        public CustomersController(ISanitizerService sanitizer, GenericRepository<Customer> customersRepository, IErrorLogService errorLogService)
        {
            _customerService = new CustomerService(sanitizer, customersRepository, errorLogService, this.userId);
        }

        [HttpGet]
        [Route("api/Customers/{id}")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            var result = await _customerService.GetById(id);

            if(result!=null)
                  return Ok(result); ;

            return BadRequest("customer not found");
        }
        [HttpPut]
        [Route("api/Customers/put")]
        public async Task<IActionResult> Put([FromBody] CustomerVM customer)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.Edit(customer);

                if (result != null)
                    return Ok(result); ;

                return BadRequest("An error occured");
            }
            return BadRequest("An error occured while trying to update customer details");
        }
    }
}
