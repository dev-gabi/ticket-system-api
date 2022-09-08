using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Services.logs;
using Services.Models.Tickets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public interface ISeedService
    {
        Task SeedDummyUsersAsync();
        Task InitTickets(HttpContext context);
  
    }
    public class SeedService: ISeedService
    {
        internal GenericRepository<Customer> CustomerRepository;
        internal GenericRepository<Employee> EmployeesRepository;
        internal GenericRepository<Ticket> TicketsRepository;
        internal GenericRepository<Reply> RepliesRepository;
        internal GenericRepository<ReplyImage> ReplyImageRepository;
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        internal HttpContext Context;
        private readonly IFileService _fileService;
        private readonly ISanitizerService _sanitizer;
        internal IWebHostEnvironment _env;
        private ITicketService _ticketService;
        private IErrorLogService _errorLogService;
        private readonly TicketStatusConfig _ticketsStatus;
        private readonly DirectoriesConfig _directories;
        private readonly TicketCategoriesConfig _categories;
        private readonly EmployeesSettingsConfig _employeesSettingsConfig;
        string[] Supporters = { "shlomo", "dave" };
        const string adminName = "ed";
        string[] Customers = { "bob", "jack", "mitch", "john", "david" };

        public SeedService(GenericRepository<Customer> customerRepository, GenericRepository<Employee> employeesRepository, GenericRepository<Ticket> ticketsRepository, GenericRepository<Reply> repliesRepository,
            GenericRepository<ReplyImage> replyImageRepository, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IFileService fileService, ISanitizerService sanitizer, IErrorLogService errorLogService,IWebHostEnvironment env,
             IOptions<TicketStatusConfig> ticketStatus, IOptions<DirectoriesConfig> directories, IOptions<EmployeesSettingsConfig> employeesSettingConfig,
             IOptions<TicketCategoriesConfig> categories)
        {
            CustomerRepository = customerRepository;
            EmployeesRepository = employeesRepository;
            TicketsRepository = ticketsRepository;
            RepliesRepository = repliesRepository;
            ReplyImageRepository = replyImageRepository;
            _fileService = fileService;
            _userManager = userManager;
            _roleManager = roleManager;
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _env = env;
            _ticketsStatus = ticketStatus.Value;
            _directories = directories.Value;
            _categories = categories.Value;
            _employeesSettingsConfig = employeesSettingConfig.Value;
        }
        void InitTicketService()
        {
            _ticketService = new TicketService(TicketsRepository, RepliesRepository, ReplyImageRepository, CustomerRepository,
                                Context.Items["Id"].ToString(), Context.Items["UserName"].ToString(), "fakeRole", _fileService,_sanitizer, _errorLogService,
                                _env, _ticketsStatus, _directories, _categories);
        }
        public Task InitTickets(HttpContext context)
        {
            Context = context;
            if (!UsersExist())
            {
                SeedDummyUsersAsync().Wait();
            }
            if (!CheckForCurrentMonthData())
            {
               return CreateTickets();
            }
            return Task.CompletedTask;
        }

        private bool UsersExist()
        {

            return EmployeesRepository.Get().Count()>0;
         }

        private bool CheckForCurrentMonthData()
        {
            return TicketsRepository.GetOne(t => t.OpenDate.Month == DateTime.Now.Month && t.OpenDate.Year == DateTime.Now.Year) != null ? true : false;
        }

        private async Task<bool> CreateTickets()
        {
            try
            {
                Customer[] customers = CustomerRepository.Get().ToList().ToArray();
                Task createDummyException = CreateExceptionRecordForLog();
                Ticket firstTicket = null;
                Ticket lastTicket = null;
                bool isfirstTicket = false;
                bool islastTicket = false;
                for (int i = 0; i < customers.Count(); i++)
                {
                    CreateTicketVM one = new() { Title = $"Button #{i + 1} not working", Message = $"When I click the {i + 1}st button nothing happens on the server. ", Category = "Back-End" };
                    Ticket ticket1 = await CreateTicket(customers[i], one);
                    Reply oneInitialReply = ticket1.Replies.First();
                    oneInitialReply.Date = DateTime.Now;
                    oneInitialReply.IsImageAttached = true;
                    RepliesRepository.Update(oneInitialReply);
                    RepliesRepository.Save();
                    if (!isfirstTicket )
                    {
                        firstTicket = ticket1;
                        isfirstTicket = true;
                    }
                    CreateTicketVM two = new() { Title = $"title on page #{i + 1} is not visible", Message = $"When I open page {i + 1} the title is not visible. ", Category = "Front-End" };
                    Ticket ticket2 = await CreateTicket(customers[i], two);
                    Reply twoInitialReply = ticket2.Replies.First();
                    twoInitialReply.Date = DateTime.Now;
                    twoInitialReply.IsImageAttached = true;
                    RepliesRepository.Update(twoInitialReply);
                    RepliesRepository.Save();

                    ReplyImageRepository.Add(new ReplyImage()
                    {
                        ReplyId = twoInitialReply.Id,
                        Path = Path.Combine(_directories.ReplyImagesAssets, "web-page.jpg")
                    });
                    ReplyImageRepository.Save();

                    CreateTicketVM three = new() { Title = $"Computer #{i + 1} not working", Message = $"the screen of computer {i + 1} is black. ", Category = "Other" };
                    Ticket ticket3 = await CreateTicket(customers[i], three);
                    Reply threeInitialReply = ticket3.Replies.First();
                    threeInitialReply.Date = DateTime.Now;
                    threeInitialReply.IsImageAttached = true;
                    RepliesRepository.Update(threeInitialReply);
                    RepliesRepository.Save();
                    if (!islastTicket)
                    {
                        lastTicket = ticket3;
                        islastTicket = true;
                    }
                }
                CreateSupportReplies().Wait();
                CloseExampleTicket(firstTicket, DateTime.Now.AddHours(1), EmployeesRepository.GetOne(e=>e.UserName == Supporters[0]).Id).Wait();
                CloseExampleTicket(lastTicket, DateTime.Now.AddHours(2), EmployeesRepository.GetOne(e => e.UserName == Supporters[1]).Id).Wait();
                await createDummyException;
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private Task CreateSupportReplies()
        {

            List<Ticket> tickets = TicketsRepository.Get(null, "Replies").ToList();
            tickets.ForEach(t =>
            {
                if(!t.Replies.First().Message.Contains("the screen of computer"))
                {
                    RepliesRepository.Add(new Reply()
                    {
                        Date = DateTime.Now.AddMinutes(5),
                        TicketId = t.Id,
                        IsImageAttached = false,
                        IsInnerReply = false,
                        Message = "Hello dear customer, This is an unexpected behevior. I am escalating your question to an Administrator",
                        UserId = EmployeesRepository.Get(e => e.UserName == Supporters[0]).FirstOrDefault().Id,
                        UserName = Supporters[0],

                    });

                    RepliesRepository.Add(new Reply()
                    {
                        Date = DateTime.Now.AddMinutes(15),
                        TicketId = t.Id,
                        IsImageAttached = false,
                        IsInnerReply = true,
                        Message = "Hi ed, please help with this issue",
                        UserId = EmployeesRepository.Get(e => e.UserName == Supporters[1]).FirstOrDefault().Id,
                        UserName = Supporters[1]
                    });

                    RepliesRepository.Add(new Reply()
                    {
                        Date = DateTime.Now.AddMinutes(25),
                        TicketId = t.Id,
                        IsImageAttached = false,
                        IsInnerReply = false,
                        Message = "Hi customer, I have fixed the problem. please clear your browser's cache and try again.",
                        UserId = EmployeesRepository.Get(e => e.UserName == adminName).FirstOrDefault().Id,
                        UserName = adminName
                    });
                }
                else
                {
                    RepliesRepository.Add(new Reply()
                    {
                        Date = DateTime.Now.AddHours(2).AddMinutes(8),
                        TicketId = t.Id,
                        IsImageAttached = false,
                        IsInnerReply = false,
                        Message = "Have you tried connecting the computer to the electricity?",
                        UserId = EmployeesRepository.Get(e => e.UserName == Supporters[1]).FirstOrDefault().Id,
                        UserName = Supporters[1]
                    });
                }

                RepliesRepository.Save();          
            });



            return Task.CompletedTask;
        }

        private Task<Ticket> CreateTicket(Customer customer, CreateTicketVM vm)
        {
            string error;
            Context.Items["Id"] = customer.Id;
            Context.Items["UserName"] = customer.UserName;
            InitTicketService();
            TicketResponse res =  _ticketService.CreateAsync(vm, out error);
            Ticket t = TicketsRepository.GetByID(res.Id);
            t.OpenDate = DateTime.Now;
            TicketsRepository.Update(t);
            TicketsRepository.Save();
            return Task.FromResult(t);
        }

     

        Task  CloseExampleTicket(Ticket ticket, DateTime closingDate, string supporterId)
        {
            ticket.Status = _ticketsStatus.Closed;
            ticket.ClosedByUser = supporterId;
            ticket.ClosingDate = closingDate;
            TicketsRepository.Update(ticket);
            TicketsRepository.Save();

            return Task.CompletedTask;
        }
        public Task SeedDummyUsersAsync()
        {
            List<IdentityUser> dummyUsers = new()
            {
                new Employee
                {
                    Email = $"{adminName}{_employeesSettingsConfig.EmailSuffix}",
                    EmailConfirmed = true,
                    UserName = adminName,
                    IsActive = true,
                    PersonalEmail = $"{adminName}@mail.com",
                    RegistrationDate = DateTime.Now
                },
                new Employee
                {
                    Email = $"{Supporters[0]}{_employeesSettingsConfig.EmailSuffix}",
                    EmailConfirmed = true,
                    UserName = Supporters[0],
                    IsActive = true,
                    PersonalEmail = $"{Supporters[0]}@mail.com",
                    RegistrationDate = DateTime.Now
                },
                new Employee
                {
                    Email = $"{Supporters[1]}{_employeesSettingsConfig.EmailSuffix}",
                    EmailConfirmed = true,
                    UserName = Supporters[1],
                    IsActive = true,
                    PersonalEmail = $"{Supporters[1]}@mail.com",
                    RegistrationDate = DateTime.Now
                },
                new Customer
                {
                    Email = $"{Customers[0]}@mail.com",
                    EmailConfirmed = true,
                    UserName = Customers[0],
                    IsActive = true,
                    Address = "Middle City, apple road 1",
                    PhoneNumber = "05011122233",
                    RegistrationDate = DateTime.Now
                },
                new Customer
                {
                    Email = $"{Customers[1]}@mail.com",
                    EmailConfirmed = true,
                    UserName = Customers[1],
                    IsActive = true,
                    Address = "Big Village, orange street 10",
                    PhoneNumber = "05044455566",
                    RegistrationDate = DateTime.Now
                },
                new Customer
                {
                    Email = $"{Customers[2]}@mail.com",
                    EmailConfirmed = true,
                    UserName = Customers[2],
                    IsActive = true,
                    Address = "Beach house 1, sea-side",
                    PhoneNumber = "05042355577",
                    RegistrationDate = DateTime.Now
                },
                new Customer
                {
                    Email = $"{Customers[3]}@mail.com",
                    EmailConfirmed = true,
                    UserName = Customers[3],
                    IsActive = true,
                    Address = "Yellow brick road, metrpolin",
                    PhoneNumber = "0521111577",
                    RegistrationDate = DateTime.Now
                },
                new Customer
                {
                    Email = $"{Customers[4]}@mail.com",
                    EmailConfirmed = true,
                    UserName = Customers[4],
                    IsActive = true,
                    Address = "1st on 1st, NY",
                    PhoneNumber = "0572222577",
                    RegistrationDate = DateTime.Now
                }
            };

            dummyUsers.ForEach(u =>
            {
                _userManager.CreateAsync(u, "a12345").Wait();

            });

            RegisterRole("Admin", dummyUsers[0]).Wait();
            RegisterRole("Supporter", dummyUsers[1]).Wait();
            RegisterRole("Supporter", dummyUsers[2]).Wait();
            RegisterRole("Customer", dummyUsers[3]).Wait();
            RegisterRole("Customer", dummyUsers[4]).Wait();
            RegisterRole("Customer", dummyUsers[5]).Wait();
            RegisterRole("Customer", dummyUsers[6]).Wait();
            RegisterRole("Customer", dummyUsers[7]).Wait();

            EmployeesRepository.Update(dummyUsers[0] as Employee);
            EmployeesRepository.Update(dummyUsers[1] as Employee);
            EmployeesRepository.Update(dummyUsers[2] as Employee);
            EmployeesRepository.Save();
            CustomerRepository.Update(dummyUsers[3] as Customer);
            CustomerRepository.Update(dummyUsers[4] as Customer);
            CustomerRepository.Update(dummyUsers[5] as Customer);
            CustomerRepository.Update(dummyUsers[6] as Customer);
            CustomerRepository.Update(dummyUsers[7] as Customer);
            CustomerRepository.Save();
            return Task.CompletedTask;
        }

        private Task CreateExceptionRecordForLog()
        {
            try
            {
                throw new Exception("Dummy error happend!");
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"Seed - CreateExceptionRecordForLog. {x.Message}", "Seed exeption example");
            }
            return Task.CompletedTask;
        }

        async Task <bool> RegisterRole(string roleName, IdentityUser identityUser)
        {
            try
            {
                    bool roleExists = _roleManager.RoleExistsAsync(roleName).Result;
                    if (!roleExists)
                    {
                        var role = new IdentityRole();
                        role.Name = roleName;
                       await _roleManager.CreateAsync(role);
                    }
                    await _userManager.AddToRoleAsync(identityUser, roleName);
                    return true;                              
            }
            catch (System.Exception)
            {
                return false;
            } 
        }
    }

}