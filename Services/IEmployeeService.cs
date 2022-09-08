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
        EmployeeResponse[] GetSupporters(out string error);
        EmployeeResponse GetEmployeeById(string id, out string error);
        List<BaseUserVM> SearchUsers(TypeAheadSearchModel model, out string error);
        EmployeeResponse EditEmployeeDetails(EmployeeEditVM vm, out string error);
        IEnumerable<SupporterStats> GetSupporterMontlyStats(SupporterStatsVM vm);
        //TopEmployeesPerformance GetTopFiveTicketClosingStats(out string error);
        GeneralMonthlyStats GetGeneralMonthlyStats(out string error);
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
        /// Gets Employee details with stats
        /// </summary>
        /// <param name="id">Employee's Id</param>
        /// <returns>EmployeeResponse Object</returns>
        public EmployeeResponse GetEmployeeById(string id, out string error)
        {
            id = _sanitizer.SanitizeString(id);
            if (string.IsNullOrEmpty(id))
            {
                _errorLogService.LogError("id is null", userId);
                error = $"id is null";
                return null;
            }
            try
            {
                EmployeeResponse employee = EmployeesRepository.GetByID(id).ConvertToEmployeeResponse();
                employee.Stats = GetSupporterMontlyStats(new SupporterStatsVM() { Id = employee.Id, Date = DateTime.Now });
                error = string.Empty;
                return employee;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetEmployeeById {x.Message} {x.InnerException}", userId);
                error = "An error has occured while trying to get user";
                return null;
            }
        }

        public EmployeeResponse[] GetSupporters(out string error)
        {
            try
            {
                IEnumerable<Employee> supporters = _userManager.GetUsersInRoleAsync(Roles.Supporter.ToString()).Result.ConvertIdentityUserListToEmployeesArray(EmployeesRepository);
                List<EmployeeResponse> response = new ();

                supporters.ToList().ForEach(s=> {
                    response.Add(s.ConvertToEmployeeResponse());
                });

                response.ForEach(e => e.Stats = GetSupporterMontlyStats(new SupporterStatsVM() { Id = e.Id, Date = DateTime.Now }));
                error = string.Empty;
                return response.ToArray();
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - SearchUsers {x.Message} {x.InnerException}", userId);
                error = "An error has occured while trying to get users";
                return null;
            }
        }

        private List<BaseUserVM> CheckIsActiveStatus(string role, List<BaseUserVM> users)
        {
            if (role == Roles.Customer.ToString())
            {
                users.ForEach(u =>
                {
                    if (!string.IsNullOrEmpty(u.Id))
                    {
                        u.IsActive = CustomerRepository.GetByID(u.Id).IsActive;
                    }
                   
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

        public EmployeeResponse EditEmployeeDetails(EmployeeEditVM vm, out string error)
        {
            if (vm == null) throw new ArgumentNullException("employee data is null");
            vm = _sanitizer.SanitizeEmploeeEditVM(vm);
            try
            {
                Employee employeetoUpdate = EmployeesRepository.GetByID(vm.Id);
                if(employeetoUpdate == null) { error = "employee not found"; return null; }
                employeetoUpdate.Email = vm.Email;
                employeetoUpdate.IsActive = vm.IsActive;
                employeetoUpdate.UserName = vm.Name;
                EmployeesRepository.Update(employeetoUpdate);
                EmployeesRepository.Save();

                EmployeeResponse res = employeetoUpdate.ConvertToEmployeeResponse();
                res.Stats = GetSupporterMontlyStats(new SupporterStatsVM() { Id = res.Id, Date = DateTime.Now });
                error = string.Empty;
                return res;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - EditEmployeeDetails {x.Message} {x.InnerException}", userId);
                error = "An error has occured while trying to edit employee";
                return null;
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
                        Replies = RepliesRepository.Get(r => r.UserId == vm.Id && r.Date.Day == date.Day && r.Date.Month==date.Month && r.Date.Year == date.Year).Count(),//todo: debug here
                        TicketsClosed = TicketsRepository.Get(t => t.ClosedByUser == vm.Id && t.ClosingDate.Date == date.Date).Count()
                    };;
                }
        }

        public  List<BaseUserVM> SearchUsers(TypeAheadSearchModel model, out string error)
        {
            model.Role = _sanitizer.SanitizeString(model.Role);
            model.SearchInput = _sanitizer.SanitizeString(model.SearchInput);
            if (! _roleManager.RoleExistsAsync(model.Role).Result)
            {
                _errorLogService.LogError($"UserService - SearchUsers. Role doesn't exist: {model.Role}", userId);
                error = $"Role {model.Role} doesn't exist for";
                return null;
            }
            try
            {
                IEnumerable<IdentityUser> users;
                if (string.IsNullOrEmpty(model.SearchInput))
                {
                     users = _userManager.GetUsersInRoleAsync(model.Role).Result;
                }
                else
                {
                    users = _userManager.GetUsersInRoleAsync(model.Role).Result.Where(n => n.UserName.Contains(model.SearchInput));
                }
               
                int usersCount = users.Count();
                int extraResults = usersCount - 5;
                List<BaseUserVM> baseUsersList = users.Take(5).ToList().ConvertIdentityUserListToBaseUserVMList(extraResults);
                error = string.Empty;
                return CheckIsActiveStatus(model.Role, baseUsersList);
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - SearchUsers {x.Message} {x.InnerException}" , userId);
                error = "An internaval server error occured while trying to search users";
                return null;
            }
        }
 
        private TopEmployeesPerformance GetTopFiveTicketClosingStats()
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
                    TopEmployeesPerformance top =  new ()
                    {
                        EmployeeStats = performanceCountByUserName,
                        MaxValue = performanceCountByUserName.GroupBy(p => p.TicketsClosed).OrderByDescending(p => p.Key).Select(p => p.Key).First()
                    };
                    return top;
                }             
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetTopFiveTicketClosingStats {x.Message} {x.InnerException}", userId);
            }
            return new TopEmployeesPerformance() { };
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

        public GeneralMonthlyStats GetGeneralMonthlyStats(out string error)
        {
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            try
            {

                GeneralMonthlyStats stats = new()
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
                    OpenedTickets = TicketsRepository.Get(t => t.OpenDate.Month == currentMonth && t.OpenDate.Year == currentYear).Count(),
                    TopPerformance = GetTopFiveTicketClosingStats()
                };
                error = string.Empty;
                return stats;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"UserService - GetGeneralMonthlyStats {x.Message} {x.InnerException}", userId);
                error = "An internaval server error occured while trying to get general monthly stats";
                return null;
            }

        }

        //public List<BaseUserVM> GetSupporters(out string error)
        //{
        //    try
        //    {
        //        List<BaseUserVM> baseUsersList = _userManager.GetUsersInRoleAsync(Roles.Supporter.ToString()).Result.ToList().ConvertIdentityUserListToBaseUserVMList(0);
        //        baseUsersList = CheckIsActiveStatus(Roles.Supporter.ToString(), baseUsersList);
        //        error = string.Empty;
        //        return baseUsersList;
        //    }
        //    catch (Exception x)
        //    {
        //        _errorLogService.LogError($"UserService - SearchUsers {x.Message} {x.InnerException}", userId);
        //        error = "An error has occured while trying to get users";
        //        return null;
        //    }
        //}
    }
}