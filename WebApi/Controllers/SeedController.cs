using Dal;
using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{

    [ApiController]
    [Route("/api/[controller]")]
    public class SeedController : Controller
    {
        internal Services.ISeedService _seed;
        internal HttpContext context;

        public SeedController(Services.ISeedService seed)
        {
            _seed = seed;
        }
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            context = ctx.HttpContext;
        }

        [HttpGet("tickets")]
        public IActionResult SeedTickets()
        {
            _seed.InitTickets(context);
            return Ok();
        }
        //[HttpGet("users")]
        //public IActionResult SeedUsers()
        //{
        //    _seed.SeedDummyUsersAsync();
        //    return Ok();
        //}
    }
}
