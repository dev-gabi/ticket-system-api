using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services.Models.logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.logs
{
    public interface IErrorLogService
    {
        void LogError(string details, string userId);
        List<Error> GetErrors(out string error);
    }
    public class ErrorLogService : IErrorLogService
    {
        private GenericRepository<ErrorLog> ErrorLogRepository;
        private UserManager<IdentityUser> _userManager;
        private readonly ConnectionsConfig _connections;

        public ErrorLogService(UserManager<IdentityUser> userManager, IOptions<ConnectionsConfig> connections)
        {
            _userManager = userManager;
            _connections = connections.Value;
        }


        public void LogError(string details, string userId)
        {            
            using (TicketsContext context = new TicketsContext(GetOptionsbuilder().Options))
            {
                ErrorLogRepository = new GenericRepository<ErrorLog>(context);

                    ErrorLogRepository.Add(
                    new ErrorLog()
                    {
                        Date = DateTime.Now
                        ,
                        Details = details,
                        UserId = userId ?? "NA"
                    });
                    ErrorLogRepository.Save();

            }
        }
        public  List<Error> GetErrors(out string error)
        {
            try
            {
                using (TicketsContext context = new TicketsContext(GetOptionsbuilder().Options))
                {
                    ErrorLogRepository = new GenericRepository<ErrorLog>(context);

                    List<Error> list = ErrorLogRepository.Get().ConvertToErrorEnumerable(_userManager).ToList();
                    error = string.Empty;
                    return list;
                }
            }
            catch (Exception)
            {
                error = "Internal error happend while getting error logs";
                return null;
            }         
        }
        private DbContextOptionsBuilder<TicketsContext>  GetOptionsbuilder()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TicketsContext>();
            optionsBuilder.UseSqlServer(_connections.DefaultConnection);
            return optionsBuilder;
        }
    }
}
