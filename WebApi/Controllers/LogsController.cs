using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using services.Enums;
using Services.logs;
using Services.Models.logs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize(new Roles[1] { Roles.Admin })]
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : BaseController
    {
        private readonly IErrorLogService _errorLogService;
        private readonly IAuthLogService _authLogService;

        public LogsController(UserManager<IdentityUser> userManager, IOptions<ConnectionsConfig> _connections, IAuthLogService authLogService)
        {
            _errorLogService = new ErrorLogService(userManager, _connections);
            _authLogService = authLogService;
        }

        [HttpGet("error-logs")]
        public async Task<List<Error>> GetErrorLogsAsync()
        {
            var result = await _errorLogService.GetErrorsAsync();
            if (result!=null)
            {
                return result;
            }
            return null;
        }
        [HttpGet("auth-logs")]
        public async Task<List<Auth>> GetAuthLogsAsync()
        {
            var result = await _authLogService.GetAuthLogs(userId);
            if (result != null)
            {
                return result;
            }
            return null;
        }
    }
}