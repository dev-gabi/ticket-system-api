using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using services.Enums;
using Services;
using Services.logs;
using Services.Models.Tickets;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Authorize]
    public class TicketsController : BaseController
    {
        internal GenericRepository<Ticket> TicketsRepository;
        internal GenericRepository<Reply> RepliesRepository;
        internal GenericRepository<ReplyImage> ReplyImageRepository;
        internal GenericRepository<Customer> CustomerRepository;
        internal GenericRepository<ErrorLog> ErrorLogRepository;
        private readonly IFileService _fileService;
        private readonly ISanitizerService _sanitizer;
        private readonly IErrorLogService _errorLogService;
        private readonly TicketStatusConfig _ticketsStatus;
        private readonly DirectoriesConfig _directories;
        private readonly TicketCategoriesConfig _categories;
        internal IWebHostEnvironment _env;
        private ITicketService _ticketService;
        private string error;

        public TicketsController(GenericRepository<Ticket> ticketsRepository, GenericRepository<Reply> repliesRepository,
            GenericRepository<ReplyImage> replyImageRepository, GenericRepository<Customer> customerRepository, GenericRepository<ErrorLog> errorLogRepository,
        IFileService fileService, ISanitizerService sanitizer, IWebHostEnvironment env, IErrorLogService errorLogService,
            IOptions<TicketStatusConfig> ticketsStatus, IOptions<DirectoriesConfig> directories, IOptions<TicketCategoriesConfig> categories)
        {
            TicketsRepository = ticketsRepository;
            RepliesRepository = repliesRepository;
            ReplyImageRepository = replyImageRepository;
            CustomerRepository = customerRepository;
            ErrorLogRepository = errorLogRepository;
            _fileService = fileService;
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _env = env;
            _ticketsStatus = ticketsStatus.Value;
            _directories = directories.Value;
            _categories = categories.Value;
        }
        void InitTicketService()
        {
            _ticketService = new TicketService(TicketsRepository, RepliesRepository, ReplyImageRepository, CustomerRepository,
                this.userId, this.userName, userRole, _fileService, _sanitizer, _errorLogService, _env, _ticketsStatus, _directories, _categories);
        }

        [HttpPost]
        [Route("api/tickets/create")]
        public IActionResult Create([Bind("Title, Message, Image, Category")][FromForm] CreateTicketVM vm)
        {
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result =  _ticketService.Create(vm, out error);
                return CreateHttpResponse(result, error);
            }
            return BadRequest(error:  ModelState.GetModelStateError());
        }

        [HttpPut]
        [Route("api/tickets/close")]
        public IActionResult Close([FromBody] IntIdModel model)
        {   
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result = _ticketService.Close(model.Id, out error);
                return CreateHttpResponse(result, error);
            }
            return BadRequest(error:  ModelState.GetModelStateError());

        }

        [HttpPost]
        [Route("api/tickets/add-reply")]
        public IActionResult AddReply([Bind("Message, TicketId, IsInnerReply, Image")][FromForm] ReplyVM vm)
        {     
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result = _ticketService.AddReply(vm, out error);
                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [HttpPost]
        [Route("api/tickets/get-by-user-id")]
        public IActionResult GetTicketsByUserId([Bind("Id, Status")][FromBody] TicketsByUser model)
        {
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result =  _ticketService.GetTicketsByUserId(model.Id, model.Status, out error);
                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpPost]
        [Route("api/tickets/get-tickets")]
        public IActionResult GetTickets([Bind("Status")] [FromBody] StatusModel model)
        {
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result =  _ticketService.GetTickets(model.Status);
                return CreateHttpResponse(result, error);
            }
            return BadRequest(error: ModelState.GetModelStateError());
        }

        [HttpGet("api/tickets/get-categories")]
        public IActionResult GetCategories()
        {
            InitTicketService();

            var response = _ticketService.GetCategories();
            if (response != null)
            {
                return Ok(response);
            }
            return Problem();
        }

    //    [HttpPost("api/tickets/type-ahead-search")]
    //    public async Task<TicketResponse[]> TypeAheadSearch( [FromBody] Services.Models.TypeAheadSearchModel model)
    //   {
    //        if (model.SearchInput!=null && model.SearchInput.Length>1)
    //        {
    //            InitTicketService();
    //            return await _ticketService.SearchByContent(model.SearchInput);
    //        }

    //        return null;
    //        }

    }
}

