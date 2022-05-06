using Dal;
using Entities;
using Microsoft.AspNetCore.Identity;
using Services.Models.logs;
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
        Task<List<Auth>> GetAuthLogs(string userId);
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
            AuthLog log = new AuthLog()
            {
                Action = actions.Logout.ToString(),
                Date = DateTime.UtcNow,
                UserId = userId,
                Details = details
            };
            SaveRecord(log);
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

        public  Task<List<Auth>> GetAuthLogs(string userId)
        {
            try
            {
                return Task.Run(()=> AuthLogRepository.Get().OrderByDescending(l => l.Date).ConvertToAuthEnumerable(_userManager).ToList());
            }
            catch (Exception x)
            {
                _errorLogService.LogError($"AuthLogService - SaveRecord {x.Message}, {x.InnerException}", userId);
                return null;
            }
        }
    }
}
