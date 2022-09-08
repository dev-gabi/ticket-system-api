using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace WebApi.Controllers
{
    [ApiController]
    public class BaseController : Controller
    {
        internal HttpContext context;
        protected string userId;
        protected string userName;
        protected string userRole;

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            context = ctx.HttpContext;

            var id = context.Items["Id"];
            if (id!=null)
            {
                userId = id.ToString();
            }
            var name = context.Items["UserName"];
            if (name!=null)
            {
                userName= name.ToString();
            }
            var role = context.Items["Role"];
            if (role != null)
            {
                userRole = role.ToString();
            }
        }

        protected IActionResult CreateHttpResponse<T>(T result, string error)
        {
            if (result != null && string.IsNullOrEmpty(error))
            {
                return Ok(result);
            }
            return Problem(detail: error);
        }
        //public HttpResponseMessage Options()
        //{
        //    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        //}
        //public HttpResponseMessage Post()
        //{
        //    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
        //}

    }
}
