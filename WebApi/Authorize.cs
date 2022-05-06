using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using services.Enums;
using System;
using System.Linq;
using WebApi.Models;

namespace WebApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly Roles[] _roles;

        public AuthorizeAttribute(Roles[] roles = null)
        {
            _roles = roles;
        }


        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Items["Id"];
            var role = context.HttpContext.Items["Role"];

            if (userId == null || (_roles != null && _roles.Any() && !Enum.IsDefined(typeof(Roles), role.ToString())))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };         
            }
        }
    }
}
