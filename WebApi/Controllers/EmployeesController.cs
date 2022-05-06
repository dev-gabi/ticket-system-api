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
        public EmployeeResponse GetEmployeeById(StringIdModel model)
        {
            return _employeeService.GetEmployeeById(model.Id);
        }

        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpPut("edit-supporter")]
        public EmployeeResponse EditSupporter([Bind("Id, Name, Email, IsActive")] EmployeeEditVM vm)
        {
            return _employeeService.EditEmployeeDetails(vm);
        }
        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpPost("search-users")]
        public async Task<IActionResult> SearchUsers([Bind("SearchInput, Role")][FromBody] TypeAheadSearchModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.SearchInput))
                {
                    return BadRequest("search input is null");
                }
                var result = await _employeeService.SearchUsers(model);

                if (result != null)
                    return Ok(result); ;

                return BadRequest("could not fetch users");
            }
            return BadRequest("An error occured while trying to fetch users");
        }
        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpGet("get-supporters")]
        public async Task<IActionResult> GetAllSupporters()
        {
                var result = await _employeeService.GetSupportersAsync();

                if (result != null)
                    return Ok(result); ;

                return BadRequest("could not fetch users");
        }
        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpGet("get-top-closing-tickets-stats")]
        public  IActionResult GetTopClosingTicketsStats()
        {
            var result = _employeeService.GetTopFiveTicketClosingStats();

            if (result != null)
                return Ok(result); 

            return BadRequest("could not fetch top closing tickets stats");
        }
        [Authorize(new Roles[1] { Roles.Admin })]
        [HttpGet("get-general-monthly-stats")]
        public IActionResult GetGeneralMonthlyStats()
        {
            var result = _employeeService.GetGeneralMonthlyStats();

            if (result != null)
                return Ok(result); ;

            return BadRequest("could not fetch monthly stats");
        }
    }
    

}
