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
        Task<List<Error>> GetErrorsAsync();
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
        public async Task<List<Error>> GetErrorsAsync()
        {
            using (TicketsContext context = new TicketsContext(GetOptionsbuilder().Options))
            {
                ErrorLogRepository = new GenericRepository<ErrorLog>(context);
                return await Task.Run(() =>
            {
                return ErrorLogRepository.Get().ConvertToErrorEnumerable(_userManager).ToList();
            });
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
