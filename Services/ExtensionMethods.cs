using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using services.Enums;
using Services.Models;
using Services.Models.Auth;
using Services.Models.Customers;
using Services.Models.Employees;
using Services.Models.logs;
using Services.Models.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    public static class ExtensionMethods
    {

        public static IEnumerable<DateTime> AllDatesInMonth(int year, int month)
        {
            foreach (var day in Enumerable.Range(1, DateTime.DaysInMonth(year, month))) { 

                yield return new DateTime(year, month, day);
            }
        }
        public static bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
        public static User ConvertToIdentityUser(this CustomerRegisterViewModel model)
        {

            return new Customer()
            {
                UserName = model.UserName,
                Email = model.Email,
                IsActive = true,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                RegistrationDate = DateTime.UtcNow
            };
        }

        public static User ConvertToIdentityUser(this EmployeeRegisterViewModel model)
        {

            return new Employee()
            {
                UserName = model.UserName,
                Email = model.Email,
                IsActive = true,
               // PersonalEmail = model.PersonalEmail,
                PhoneNumber = model.PhoneNumber,
                RegistrationDate = DateTime.UtcNow
            };
        }

        public static User ConvertToIdentityUser(this EmployeePreRegisterViewModel model)
        {
            return new Employee()
            {
                UserName = model.UserName,
                PersonalEmail = model.PersonalEmail

            };
        }

        public static CustomerVM ConvertToCustomerVM(this Customer customer, string customerRole)
        {
            return new CustomerVM()
            {
                Id = customer.Id.ToString(),
                Email = customer.Email,
                Address = customer.Address,
                Role = customerRole,
                IsActive = customer.IsActive,
                Name = customer.UserName,
                PhoneNumber = customer.PhoneNumber,
                RegistrationDate = customer.RegistrationDate
            };
        }

        public static List<CustomerVM> ConvertToCustomerVMList(this List<Customer> entityList, string customerRole)
        {
            List<CustomerVM> vmList = new();
            entityList.ForEach(e =>
            {
                vmList.Add(e.ConvertToCustomerVM(customerRole));
            });
            return vmList;
        }
        public static List<BaseUserVM> ConvertIdentityUserListToBaseUserVMList(this List<IdentityUser> identityList)
        {
            List<BaseUserVM> vmList = new();
            identityList.ForEach(i =>
            {
                vmList.Add(new BaseUserVM()
                {
                    Id = i.Id,
                    Name = i.UserName
                });
            });
            return vmList;
        }

        public static TicketResponse[] ConvertToTicketResponseArray(this IEnumerable<Ticket> tickets, GenericRepository<ReplyImage> replyImageRepository)
        {
            return GetTicketsResponseList(tickets, replyImageRepository).ToArray();
        }



        public static List<TicketResponse> ConvertToTicketResponseList(this IEnumerable<Ticket> tickets, GenericRepository<ReplyImage> replyImageRepository)
        {
            return GetTicketsResponseList(tickets, replyImageRepository);
        }
        private static List<TicketResponse> GetTicketsResponseList(IEnumerable<Ticket> tickets, GenericRepository<ReplyImage> replyImageRepository)
        {
            List<Ticket> ticketsList = tickets.ToList();
            List<TicketResponse> responseList = new List<TicketResponse>();
            ticketsList.ForEach(t =>
            {
                TicketResponse res = t.ConvertToTicketResponse();
                res.Replies = GetRepliesImages(res.Replies, replyImageRepository);
                responseList.Add(t.ConvertToTicketResponse());
            });
            return responseList;
        }
        private static Reply[] GetRepliesImages(Reply[] replies, GenericRepository<ReplyImage> replyImageRepository)
        {
            foreach (Reply reply in replies)
            {
                if (reply.IsImageAttached)
                {
                    reply.Image = replyImageRepository.GetOne(i => i.ReplyId == reply.Id);
                }
            }
            return replies;
        }
        static TicketResponse ConvertToTicketResponse(this Ticket t)
        {
            return new TicketResponse()
            {
                ClosedByUser = t.ClosedByUser,
                ClosingDate = t.ClosingDate,
                CustomerId = t.CustomerId,
                Category = t.Categoty,
                Id = t.Id,
                OpenDate = t.OpenDate,
                Status = t.Status,
                Title = t.Title,
                Replies = t.Replies.OrderBy(r=>r.Date).ToArray()
            };
            
        }

        public static AddReplyResponse ConvertToAddReplyResponse(this Reply r)
        {
            AddReplyResponse res = new AddReplyResponse()
            {
                Date = r.Date,
                IsImageAttached = r.IsImageAttached,
                Message = r.Message,
                ReplyId = r.Id,
                TicketId = r.TicketId,
                UserId = r.UserId,
                UserName = r.UserName,
                IsInnerReply = r.IsInnerReply
            };
            if (r.IsImageAttached)
            {
                res.ImagePath = r.Image.Path;
            }
            return res;
        }
        public static EmployeeResponse ConvertToEmployeeResponse(this Employee e)
        {
            return new EmployeeResponse()
            {
                Email = e.Email,
                Id = e.Id,
                IsActive = e.IsActive,
                Name = e.UserName,
                RegistrationDate = e.RegistrationDate,

            };
        }
        public static IEnumerable<Error> ConvertToErrorEnumerable(this IEnumerable<ErrorLog> logs, UserManager<IdentityUser> _userManager)
        {
            foreach (var log in logs)
            {
                string userName;
                IdentityUser user = _userManager.FindByIdAsync(log.UserId).Result;
                  if (user != null) { userName = user.UserName; } else { userName = "guest"; }

                yield return new Error()
                {
                    Date = log.Date,
                    ErrorDetails = log.Details,
                    UserName = userName
                };
            }
        }
        public static IEnumerable<Auth> ConvertToAuthEnumerable(this IEnumerable<AuthLog> logs, UserManager<IdentityUser> _userManager)
        {
            foreach (var log in logs)
            {
                string userName;
                IdentityUser user = _userManager.FindByIdAsync(log.UserId).Result;
                if (user != null) { userName = user.UserName; } else { userName = "guest"; }

                yield return new Auth()
                {
                    Date = log.Date,
                    Details = log.Details,
                    UserName = userName,
                    Action = log.Action
                };
            }
        }
        public static IEnumerable<TopEmployeesPerformanceByName> ConvertToTopEmployeesIdToName(this List<TopEmployeesPerformanceById> idList) 
        {
            foreach(var item in idList)
            {
                yield return new TopEmployeesPerformanceByName()
                {
                    TicketsClosed = item.TicketsClosed,
                    UserName = item.Id //id value was already converted to user name
                };
            }
        }
    }
}

