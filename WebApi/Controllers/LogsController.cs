using Entities.configutation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using services.Enums;
using Services.logs;

namespace WebApi.Controllers
{
    [Authorize(new Roles[1] { Roles.Admin })]
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : BaseController
    {
        private readonly IErrorLogService _errorLogService;
        private readonly IAuthLogService _authLogService;
        private string error;
        public LogsController(UserManager<IdentityUser> userManager, IOptions<ConnectionsConfig> _connections, IAuthLogService authLogService)
        {
            _errorLogService = new ErrorLogService(userManager, _connections);
            _authLogService = authLogService;
        }

        [HttpGet("error-logs")]
        public IActionResult GetErrorLogs()
        {
            var result =  _errorLogService.GetErrors(out error);
            return CreateHttpResponse(result, error);

        }
        [HttpGet("auth-logs")]
        public  IActionResult GetAuthLogsAsync()
        {
            var result =  _authLogService.GetAuthLogs(userId, out error);
            return CreateHttpResponse(result, error);
        }
    }
}