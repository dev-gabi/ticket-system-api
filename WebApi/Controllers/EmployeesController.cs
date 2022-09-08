using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using services.Enums;
using Services;
using Services.logs;
using Services.Models;
using Services.Models.Employees;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : BaseController
    {
            
        private IEmployeeService _employeeService;
        private string error;
        public EmployeesController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            GenericRepository<Employee> employeesRepository, GenericRepository<Customer> customerRepository,
            GenericRepository<Ticket> ticketsRepository, GenericRepository<Reply> repliesRepository, GenericRepository<ErrorLog> errorLogrepository,
            ISanitizerService sanitizer, IErrorLogService errorLogService, IOptions<TicketStatusConfig> ticketStatusconfig)
        {
            _employeeService = new EmployeeService(userManager, roleManager, employeesRepository, customerRepository, ticketsRepository,
                repliesRepository, errorLogrepository, sanitizer, errorLogService, ticketStatusconfig.Value, this.userId);
        }

        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpPost("get-by-id")]
        public IActionResult GetEmployeeById(StringIdModel model)
        {
            if (ModelState.IsValid) { 
            
                var result = _employeeService.GetEmployeeById(model.Id, out error);

                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpPost("search-users")]
        public IActionResult SearchUsers([Bind("SearchInput, Role")][FromBody] TypeAheadSearchModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _employeeService.SearchUsers(model, out error);

                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpGet("get-supporters")]
        public IActionResult GetAllSupporters()
        {
            var result = _employeeService.GetSupporters(out error);

            return CreateHttpResponse(result, error);
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpPut("edit-supporter")]
        public IActionResult EditSupporter([Bind("Id, Name, Email, IsActive")] EmployeeEditVM vm)
        {        
            if (ModelState.IsValid)
            {
               var result =_employeeService.EditEmployeeDetails(vm, out error);

                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpGet("get-general-monthly-stats")]
        public IActionResult GetGeneralMonthlyStats()
        {
            var result = _employeeService.GetGeneralMonthlyStats(out error);

            return CreateHttpResponse(result, error);
        }
        //deprecated - now a part of GetGeneralMonthlyStats response
        //[Authorize(new Roles[1] { Roles.Admin })]
        //[HttpGet("get-top-closing-tickets-stats")]
        //public  IActionResult GetTopClosingTicketsStats()
        //{
        //    var result = _employeeService.GetTopFiveTicketClosingStats(out error);

        //    return CreateHttpResponse(result, error);
        //}
    }
}