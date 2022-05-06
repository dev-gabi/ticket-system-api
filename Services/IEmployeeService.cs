using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using services.Enums;
using Services.logs;
using Services.Models;
using Services.Models.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public interface IEmployeeService
    {
        Task<List<BaseUserVM>> GetSupportersAsync();
        EmployeeResponse GetEmployeeById(string id);
        Task<List<BaseUserVM>> SearchUsers(TypeAheadSearchModel model);
        EmployeeResponse EditEmployeeDetails(EmployeeEditVM vm);
        IEnumerable<SupporterStats> GetSupporterMontlyStats(SupporterStatsVM vm);
        TopEmployeesPerformance GetTopFiveTicketClosingStats();
        GeneralMonthlyStats GetGeneralMonthlyStats();
    }

    public class EmployeeService : IEmployeeService
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        internal GenericRepository<Employee> EmployeesRepository;
        internal GenericRepository<Customer> CustomerRepository;
        internal GenericRepository<Ticket> TicketsRepository;
        internal GenericRepository<Reply> RepliesRepository;
        internal GenericRepository<ErrorLog> ErrorLogRepository;
        private readonly ISanitizerService _sanitizer;
        private readonly IErrorLogService _errorLogService;
        private readonly TicketStatusConfig _ticketStatusConfig;
        private string userId;

        public EmployeeService(UserManager<IdentityUser> userManager,  RoleManager<IdentityRole> roleManager,
            GenericRepository<Employee> employeesRepository , GenericRepository<Customer> customerRepository,
            GenericRepository<Ticket> ticketsRepository, GenericRepository<Reply>  repliesRepository, GenericRepository<ErrorLog> errorLogRepository,
            ISanitizerService sanitizer, IErrorLogService errorLogService, TicketStatusConfig ticketStatusConfig, string _userId)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            EmployeesRepository = employeesRepository;
            CustomerRepository = customerRepository;
            TicketsRepository = ticketsRepository;
            RepliesRepository = repliesRepository;
            ErrorLogRepository = errorLogRepository;
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _ticketStatusConfig = ticketStatusConfig;
            userId = _userId;
        }
        /// <summary>
        /// Gets Employee details with previous month stats
        /// </summary>
        /// <param name="id">Employee's Id</param>
        /// <returns>EmployeeResponse Object</returns>
        public EmployeeResponse GetEmployeeById(string id)
        {
            id = _sanitizer.SanitizeString(id);
            if (string.IsNullOrEmpty(id))
            {
                _errorLogService.LogError("id is null", userId);
                return new EmployeeResponse() { Error = "id is null" };
            }
            try
            {
                EmployeeResponse employee = EmployeesRepository.GetByID(id).ConvertToEmployeeResponse();
                DateTime now = DateTime.Now;
                employee.Stats = GetSupporterMontlyStats(new SupporterStatsVM() { Id = employee.Id, Date = now });
                return employee;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetEmployeeById {x.Message} {x.InnerException}", userId);
                return new EmployeeResponse() { Error = "An error has occured while trying to get user" };
            }

        }


        private List<BaseUserVM> CheckIsActiveStatus(string role, List<BaseUserVM> users)
        {
            if (role == Roles.Customer.ToString())
            {
                users.ForEach(u =>
                {
                    u.IsActive = CustomerRepository.GetByID(u.Id).IsActive;
                });
                return users;
            }
            else if (role == Roles.Supporter.ToString())
            {
                users.ForEach(u =>
                {
                    u.IsActive = EmployeesRepository.GetByID(u.Id).IsActive;
                });
                return users;
            }
            else
            {
                _errorLogService.LogError($"UserService - CheckIsActiveStatus. Invalid role: {role}" , userId);
                return null;
            }
        }

        public EmployeeResponse EditEmployeeDetails(EmployeeEditVM vm)
        {
            if (vm == null) throw new ArgumentNullException("employee data is null");
            vm = _sanitizer.SanitizeEmploeeEditVM(vm);
            try
            {
                Employee employeetoUpdate = EmployeesRepository.GetByID(vm.Id);
                if(employeetoUpdate == null) { return new EmployeeResponse() { Error = "employee not found" }; }
                employeetoUpdate.Email = vm.Email;
                employeetoUpdate.IsActive = vm.IsActive;
                employeetoUpdate.UserName = vm.Name;
                EmployeesRepository.Update(employeetoUpdate);
                EmployeesRepository.Save();
                return employeetoUpdate.ConvertToEmployeeResponse();
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - EditEmployeeDetails {x.Message} {x.InnerException}", userId);
                return new EmployeeResponse() { Error = "An error has occured while trying to edit employee" };
            }
        }

        public IEnumerable<SupporterStats> GetSupporterMontlyStats(SupporterStatsVM vm)
        {
            if (vm == null) throw new ArgumentNullException("employee data is null");
            vm = _sanitizer.SanitizeSupporterStatsVM(vm);

                List<DateTime> daysInMonth = ExtensionMethods.AllDatesInMonth(vm.Date.Year, vm.Date.Month).ToList();

                foreach (var date in daysInMonth)
                {
                   yield return new SupporterStats()
                    {
                        Date = date,
                        Replies = RepliesRepository.Get(r => r.UserId == vm.Id && r.Date == date.Date).Count(),//todo: debug here
                        TicketsClosed = TicketsRepository.Get(t => t.ClosedByUser == vm.Id && t.ClosingDate.Date == date.Date).Count()
                    };;
                }
        }

        public async Task<List<BaseUserVM>> SearchUsers(TypeAheadSearchModel model)
        {
            model.Role = _sanitizer.SanitizeString(model.Role);
            model.SearchInput = _sanitizer.SanitizeString(model.SearchInput);
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                _errorLogService.LogError($"UserService - SearchUsers. Role doesn't exist: {model.Role}", userId);
                return null;
            }
            try
            {
                List<IdentityUser> users = _userManager.GetUsersInRoleAsync(model.Role).Result.Where(n=>n.UserName.Contains(model.SearchInput)).ToList();
                List<BaseUserVM> baseUsersList = users.ConvertIdentityUserListToBaseUserVMList();
                return CheckIsActiveStatus(model.Role, baseUsersList);
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - SearchUsers {x.Message} {x.InnerException}" , userId);
                return null;
            }
        }

        public async Task<List<BaseUserVM>> GetSupportersAsync()
        {
            try
            {
                List<BaseUserVM> baseUsersList =  _userManager.GetUsersInRoleAsync(Roles.Supporter.ToString()).Result.ToList().ConvertIdentityUserListToBaseUserVMList();
                return await Task.FromResult(CheckIsActiveStatus(Roles.Supporter.ToString(), baseUsersList));
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - SearchUsers {x.Message} {x.InnerException}", userId);
                return null;
            }        
        }
        
        public TopEmployeesPerformance GetTopFiveTicketClosingStats()
       {
            try
            {
                List<Ticket> currentMonthClosedTickets = TicketsRepository.Get(t => t.Status == _ticketStatusConfig.Closed && t.ClosingDate.Month == DateTime.Now.Month && t.ClosingDate.Year == DateTime.Now.Year).ToList();
                currentMonthClosedTickets = RemoveTicketsClosedByCustomers(currentMonthClosedTickets).ToList();
                List<TopEmployeesPerformanceById> performanceCountByUserId = currentMonthClosedTickets.GroupBy(t => t.ClosedByUser).OrderByDescending(t => t.Key).Select(u => new TopEmployeesPerformanceById { Id = u.Key, TicketsClosed = u.Count() }).Take(5).ToList();
                performanceCountByUserId.ForEach(p =>
                {
                    Employee e = EmployeesRepository.GetByID(p.Id);
                    if (e != null)
                        {
                        p.Id = e.UserName;
                    }
                    else { p.Id = null; }                
                });
   
                IEnumerable<TopEmployeesPerformanceByName> performanceCountByUserName = performanceCountByUserId.ConvertToTopEmployeesIdToName();

                if (performanceCountByUserName?.Any() == true)
                {
                    return new TopEmployeesPerformance()
                    {
                        EmployeeStats = performanceCountByUserName,
                        MaxValue = performanceCountByUserName.GroupBy(p => p.TicketsClosed).OrderByDescending(p => p.Key).Select(p => p.Key).First()
                    };
                }
                return new TopEmployeesPerformance() { };

            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetTopFiveTicketClosingStats {x.Message} {x.InnerException}", userId);
                return null;
            }
 
        }

        private IEnumerable<Ticket> RemoveTicketsClosedByCustomers(List<Ticket> currentMonthClosedTickets)
        {
            IEnumerable<string> usersIdList = ExtractUserIdListFromClosedTickets(currentMonthClosedTickets);
                foreach (string id in usersIdList)
                {
                    if (EmployeesRepository.GetByID(id) != null)
                    {
                        yield return currentMonthClosedTickets.Where(t => t.ClosedByUser == id).FirstOrDefault();
                    }
                 }
        }

        private IEnumerable<string> ExtractUserIdListFromClosedTickets(List<Ticket> currentMonthClosedTickets)
        {
            foreach (var t in currentMonthClosedTickets)
            {
                yield return t.ClosedByUser;
            }
        }

        public GeneralMonthlyStats GetGeneralMonthlyStats()
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            try
            {
                return new GeneralMonthlyStats()
                {
                    TotalClosedTickets = TicketsRepository.Get(
                        t => t.Status == _ticketStatusConfig.Closed
                        && t.ClosingDate.Month == currentMonth && t.ClosingDate.Year == currentYear
                        ).Count(),
                    ClosedTicketsThatWereOpenThisMonth = TicketsRepository.Get(
                        t => t.Status == _ticketStatusConfig.Closed
                            && t.ClosingDate.Month == currentMonth && t.ClosingDate.Year == currentYear
                            && t.OpenDate.Month == currentMonth && t.OpenDate.Year == currentYear
                        ).Count(),
                    TotalReplies = RepliesRepository.Get(r => r.Date.Month == currentMonth && r.Date.Year == currentYear).Count(),
                    OpenedTickets = TicketsRepository.Get(t => t.OpenDate.Month == currentMonth && t.OpenDate.Year == currentYear).Count()
                };
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetGeneralMonthlyStats {x.Message} {x.InnerException}", userId);
                return null;
            }

        }
    }
}