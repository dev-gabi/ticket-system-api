using Dal;
using Entities;
using Microsoft.AspNetCore.Identity;
using Services.Models.Logs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.logs
{
    public interface IAuthLogService
    {
        void LogLogin(string userId, string details);
        void LogLogout(string userId, string details);
        List<Auth> GetAuthLogs(string userId, out string error);
    }
    public class AuthLogService : IAuthLogService
    {
        enum actions{
            Login,
            Logout
        }

        GenericRepository<AuthLog> AuthLogRepository;
        private readonly IErrorLogService _errorLogService;
        private UserManager<IdentityUser> _userManager;

        public AuthLogService(UserManager<IdentityUser> userManager, GenericRepository<AuthLog> authLogRepository, IErrorLogService errorLogService)
        {
            _userManager = userManager;
            AuthLogRepository = authLogRepository;
            _errorLogService = errorLogService;
        }
        public void LogLogin(string userId, string details)
        {
            AuthLog log = new AuthLog()
            {
                Action = actions.Login.ToString(),
                Date = DateTime.UtcNow,
                UserId = userId,
                Details = details
            };
            SaveRecord(log);
        }

        public void LogLogout(string userId, string details)
        {
            try
            {
                AuthLog log = new AuthLog()
                {
                    Action = actions.Logout.ToString(),
                    Date = DateTime.UtcNow,
                    UserId = userId,
                    Details = details
                };
                SaveRecord(log);
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void SaveRecord(AuthLog log)
        {
            try
            {
                AuthLogRepository.Add(log);
                AuthLogRepository.Save();
            }
            catch (Exception x)
            {
                string id = log.UserId ?? "undefined";
                _errorLogService.LogError($"AuthLogService - SaveRecord {x.Message}, {x.InnerException}", id);
            }

        }

        public  List<Auth> GetAuthLogs(string userId, out string error)
        {
            try
            {
   
                List<Auth> list = AuthLogRepository.Get().OrderByDescending(l => l.Date).ConvertToAuthEnumerable(_userManager).ToList();
                error = string.Empty;
                return list;
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthLogService - SaveRecord {x.Message}, {x.InnerException}", userId);
                error = "An internal server error occured while getting auth logs";
                return null;
            }
        }
    }
}
