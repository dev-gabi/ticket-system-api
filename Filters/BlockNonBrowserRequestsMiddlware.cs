using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Filters
{
    public class BlockNonBrowserRequestsMiddlware
    {
        private readonly RequestDelegate _next;

        public BlockNonBrowserRequestsMiddlware(RequestDelegate next)
        {
            _next = next;
        }


        public  Task Invoke(HttpContext context)
        {
            string userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();

            if (userAgent.Contains("Chrome") || userAgent.Contains("Firefox"))
            {
                return _next(context);
            }
            context.Response.StatusCode = 401;
            return context.Response.WriteAsync("access forbidden");


        }
    }
}
