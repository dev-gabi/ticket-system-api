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
        public async Task<TicketResponse> Create([Bind("Title, Message, Image, Category")][FromForm] CreateTicketVM vm)
        {
            if (ModelState.IsValid)
            {
                InitTicketService();
                var result = await _ticketService.CreateAsync(vm);
                if (result != null)
                    return result;
            }
            return new TicketResponse() { };
        }

        [HttpPut]
        [Route("api/tickets/close")]
        public IActionResult Close([FromBody] IntIdModel model)
        {
            InitTicketService();
            var response = _ticketService.Close(model.Id);
            if (response != null)
                return Ok(response);

            return BadRequest($"An error occured while trying to close ticket {model.Id}");
        }

        [HttpPost]
        [Route("api/tickets/add-reply")]
        public async Task<AddReplyResponse> AddReply([Bind("Message, Image, TicketId, IsInnerReply")][FromForm] ReplyVM vm)
        {
            InitTicketService();
            if (ModelState.IsValid)
            {
                AddReplyResponse response = await _ticketService.AddReplyAsync(vm);
                if (response != null)
                    return response;
            }
            return new AddReplyResponse() { Error = "An error occured while trying to add a ticket reply" };
        }


        [HttpPost]
        [Route("api/tickets/get-all-by-user-id")]
        public TicketResponse[] GetAllByUserId([FromBody] StringIdModel model)
        {
            InitTicketService();
            return _ticketService.GetTicketsByUserId(model.Id, false);
        }
        [HttpPost]
        [Route("api/tickets/get-open-tickets-by-user-id")]
        public TicketResponse[] GetOpenTicketsByUserId([FromBody] StringIdModel model)
        {
            InitTicketService();
            return _ticketService.GetTicketsByUserId(model.Id, true);
        }

        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpGet]
        [Route("api/tickets/get-all")]
        public async Task<TicketResponse[]> GetAll()
        {
            InitTicketService();
            return await _ticketService.GetAll();
        }

        [Authorize(new Roles[2] { Roles.Admin, Roles.Supporter })]
        [HttpGet]
        [Route("api/tickets/get-open-tickets")]
        public async Task<TicketResponse[]> GetOpenTickets()
        {
            InitTicketService();
            return await _ticketService.GetOpenTickets();
        }


        [HttpGet("api/tickets/get-categories")]
        public string[] GetCategories()
        {
            InitTicketService();
            return _ticketService.GetCategories();
        }

        [HttpPost("api/tickets/type-ahead-search")]
        public async Task<TicketResponse[]> TypeAheadSearch( [FromBody] Services.Models.TypeAheadSearchModel model)
        {
            if (model.SearchInput!=null && model.SearchInput.Length>1)
            {
                InitTicketService();
                return await _ticketService.SearchByContent(model.SearchInput);
            }

            return null;
            }
    }
}

