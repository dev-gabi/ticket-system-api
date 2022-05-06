using Dal;
using Entities;
using Entities.configutation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using services.Enums;
using Services.Helpers;
using Services.logs;
using Services.Models.Tickets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace Services
{
    public interface ITicketService
    {
        Task<TicketResponse> CreateAsync(CreateTicketVM vm);
        Task<AddReplyResponse> AddReplyAsync(ReplyVM vm);
        ApiResponse Close(int id);
        TicketResponse[] GetTicketsByUserId(string id, bool onlyOpenTickets);
        Task<TicketResponse[]> GetAll();
        Task<TicketResponse[]> GetOpenTickets();
        string[] GetCategories();
        Task<TicketResponse[]> SearchByContent(string searchInput);

    }

    public class TicketService :  ITicketService
    {
        internal GenericRepository<Ticket> TicketsRepository;
        internal GenericRepository<Reply> RepliesRepository;
        internal GenericRepository<ReplyImage> ReplyImageRepository;
        internal GenericRepository<Customer> CustomersRepository;
        private IFileService _fileService;
        private IErrorLogService _errorLogService;
        private ISanitizerService _sanitizer;
        private readonly TicketStatusConfig _ticketsStatus;
        private readonly DirectoriesConfig _directories;
        private readonly TicketCategoriesConfig _categories;
        internal IWebHostEnvironment _env;
        private string _userId;
        private string _userName;
        private string _userRole;

        public TicketService(GenericRepository<Ticket> ticketsRepository, GenericRepository<Reply> repliesRepository,
            GenericRepository<ReplyImage> replyImageRepository, GenericRepository<Customer> customersRepository, 
            string userId,string userName,string userRole,
            IFileService fileService, ISanitizerService sanitizer, IErrorLogService errorLogService, IWebHostEnvironment env,
            TicketStatusConfig ticketsStatus, DirectoriesConfig directories, TicketCategoriesConfig categories)
        {
            TicketsRepository = ticketsRepository;
            RepliesRepository = repliesRepository;
            ReplyImageRepository = replyImageRepository;
            CustomersRepository = customersRepository;
            _fileService = fileService;
            _sanitizer = sanitizer;
            _errorLogService = errorLogService;
            _env = env;
            _ticketsStatus = ticketsStatus;
            _directories = directories;
            _categories = categories;
            _userId = userId;
            _userName = userName;
            _userRole = userRole;
        }

        public async Task<TicketResponse> CreateAsync(CreateTicketVM vm)
        {
            try
            {
                vm = _sanitizer.SanitizeCreateTicketViewModel(vm);

                Ticket ticketEntity = SaveTicketAndGetEntity(vm);
                ReplyVM replyVm = new ReplyVM() { Message = vm.Message, TicketId = ticketEntity.Id, Image = vm.Image };
                Reply reply = SaveReplyToDBAsync(replyVm);
                if (vm.Image != null)
                {
                    reply.IsImageAttached = true;
                    await SaveReplyImage(replyVm, reply);
                }
                return CreateTicketResponse(ticketEntity, reply);
            }
            catch (Exception e)
            {
                _errorLogService.LogError("TicketService - CreateAsync: " + e.ToString(), _userId);
                return null;
            }

       
        }

        private TicketResponse CreateTicketResponse(Ticket ticketEntity, Reply reply)
        {
            return new TicketResponse
            {
                Id = ticketEntity.Id,
                CustomerId = _userId,
                CustomerName = _userName,
                Category = ticketEntity.Categoty,
                OpenDate = ticketEntity.OpenDate,
                Status = ticketEntity.Status,
                Title = ticketEntity.Title,
                Replies = new Reply[1] { reply }
            };
        }

        private Ticket SaveTicketAndGetEntity(CreateTicketVM vm)
        {
            try
            {
                Ticket t = new Ticket()
                {
                    Title = vm.Title,
                    Categoty = vm.Category,
                    OpenDate = DateTime.Now,
                    Status = _ticketsStatus.Open,
                    CustomerId = _userId
                };
                TicketsRepository.Add(t);
                TicketsRepository.Save();
                return t;
            }
            catch (Exception e)
            {
                _errorLogService.LogError("TicketService - SaveTicketAndGetEntity: " + e.ToString(), _userId);
                return null;
            }

        }

        private async Task SaveReplyImage(ReplyVM vm, Reply reply)
        {
            bool imagesSaved = await AddReplyImage(vm.Image, reply.Id, out ReplyImage replimage);
            if (imagesSaved)
            {
                try
                {
                    RepliesRepository.Update(reply);
                    RepliesRepository.Save();
                }
                catch (Exception e)
                {
                    _errorLogService.LogError($"TicketService - SaveReplyImage: {e.Message} {e.InnerException}", _userId);                  
                }

            }
        }

        Reply SaveReplyToDBAsync(ReplyVM vm)
        {
            try
            {
                Reply r = new Reply()
                {
                    Date = DateTime.Now,
                    Message = vm.Message,
                    TicketId = vm.TicketId,
                    UserId = _userId,
                    UserName = _userName,
                    IsInnerReply = vm.IsInnerReply
                };

                RepliesRepository.Add(r);
                RepliesRepository.Save();
                return r;
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService - SaveReplyToDBAsync: {e.Message} {e.InnerException}", _userId);
                return null;
            }

        }

        public async Task<AddReplyResponse> AddReplyAsync(ReplyVM vm)
        {
            vm.Message = _sanitizer.SanitizeString(vm.Message);
            try
            {
                Reply r =   SaveReplyToDBAsync(vm);
                if (vm.Image != null)
                {
                    r.IsImageAttached = true;
                    await SaveReplyImage(vm, r);
                }
                return r.ConvertToAddReplyResponse();
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService - SaveReplyToDBAsync: {e.Message} {e.InnerException}" , _userId);
                return new AddReplyResponse() { Error = "An error has occured while trying to create a new reply" };
            }

        }

        private Task<bool> AddReplyImage(IFormFile image, int replyId, out ReplyImage replyImage)
        {
            try
            {
                string uploadPath = _directories.ReplyImagesUpload;
                string fileName = DateTime.Now.ToBinary().ToString() + image.FileName.Trim().Replace(' ', '-');
                bool uploadImage = _fileService.UploadFile(uploadPath, image, fileName, _userId).Result;
                ReplyImage ri = new ReplyImage()
                {
                    ReplyId = replyId,
                    Path = Path.Combine(_directories.ReplyImagesAssets, fileName)
                };
                ReplyImageRepository.Add(ri);
                ReplyImageRepository.Save();
                replyImage = ri;
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService - AddReplyImage: {e.Message} {e.InnerException}", _userId);
            }
            replyImage = null;
            return Task.FromResult(false);
        }

        public ApiResponse Close(int id)
        {
            Ticket t = TicketsRepository.GetByID(id);
            if (t.Status == _ticketsStatus.Closed)
                return ResponseHelpers.ApiResponseError($"Ticket {id} was already closed.");
            try
            {
                t.ClosingDate = DateTime.Now;
                t.Status = _ticketsStatus.Closed;
                t.ClosedByUser = _userId;
                TicketsRepository.Update(t);
                TicketsRepository.Save();
                return ResponseHelpers.ApiResponseSuccess("Ticket is now closed");
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService - Close: {e.Message} {e.InnerException}" , _userId);
            }
            return ResponseHelpers.ApiResponseError($"An error has occured while trying to close ticket: {id}");
        }

        public TicketResponse[] GetTicketsByUserId(string id, bool onlyOpenTickets)
        {
            id = _sanitizer.SanitizeString(id);
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Id is null");
            try
            {
                List<TicketResponse> tickets;
                if (onlyOpenTickets)
                {
                    tickets = TicketsRepository.Get(t => t.CustomerId == id && t.Status == _ticketsStatus.Open, "Replies").ConvertToTicketResponseList(ReplyImageRepository);
                }
                else
                {
                    tickets = TicketsRepository.Get(t => t.CustomerId == id, "Replies").ConvertToTicketResponseList(ReplyImageRepository);
                }
                return tickets.ToArray();
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService - GetTicketsByUserId: {e.Message} {e.InnerException}", _userId);
                return null;
            }
        }

        public Task<TicketResponse[]> GetAll()
        {
            try
            {
                TicketResponse[] tickets = TicketsRepository.Get(null, "Replies").ConvertToTicketResponseArray(ReplyImageRepository);
                return Task.FromResult(GetCustomerNamesForTickets(tickets));
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService -  GetAll: {e.Message} {e.InnerException}" , _userId);
                return null;
            }
        }

        public Task<TicketResponse[]> GetOpenTickets()
        {
            try
            {
                TicketResponse[] tickets = TicketsRepository.Get(t => t.Status == _ticketsStatus.Open, "Replies").ConvertToTicketResponseArray(ReplyImageRepository);
                return Task.FromResult(GetCustomerNamesForTickets(tickets));
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService -  GetOpenTickets: {e.Message} {e.InnerException}", _userId);
                return null;
            }
        }

        private TicketResponse[] GetCustomerNamesForTickets(TicketResponse[] tickets)
        {
            if (tickets == null) { return null; }
            for (int i = 0; i < tickets.Length; i++)
            {
                tickets[i].CustomerName = CustomersRepository.GetByID(tickets[i].CustomerId).UserName;
            }
            return tickets;
        }

        public string[] GetCategories()
        {
            List<string> categoryList = new List<string>();
            foreach (PropertyInfo prop in _categories.GetType().GetProperties())
            {
                categoryList.Add((string)prop.GetValue(_categories, null));
            }
            return categoryList.ToArray();
        }

        public Task<TicketResponse[]> SearchByContent(string searchInput)
        {
            searchInput = _sanitizer.SanitizeString(searchInput);
            if (string.IsNullOrEmpty(searchInput)) { throw new ArgumentNullException("search input is null"); }
            try
            {
            IEnumerable<TicketResponse> titleContainingSearchInput = GetTicketsByTitleContent(searchInput);
            IEnumerable<TicketResponse> repliesContainingSearchInput = GetTicketsByRepliesContent(searchInput);
            IEnumerable<TicketResponse> resultList = (titleContainingSearchInput ?? Enumerable.Empty<TicketResponse>()).Concat(repliesContainingSearchInput ?? Enumerable.Empty<TicketResponse>());
            
            return Task.FromResult(GetCustomerNamesForTickets(resultList.GroupBy(t=>t.Id).Select(t=>t.First()).ToArray()));
            }
            catch (Exception e)
            {
                _errorLogService.LogError($"TicketService -  SearchByContent: {e.Message} {e.InnerException}", _userId);
                return null;
            }
        }

        private IEnumerable<TicketResponse> GetTicketsByRepliesContent(string searchInput)
        {
            IEnumerable<Ticket> tickets = GetTicketsByReplies(RepliesRepository.Get(r => r.Message.Contains(searchInput)));
            return tickets.ConvertToTicketResponseArray(ReplyImageRepository);
        }

        private IEnumerable<Ticket> GetTicketsByReplies(IEnumerable<Reply> replies)
        {   
            if (_userRole == Roles.Customer.ToString())
            {  
                foreach (var reply in replies)
                {
                    string userIdOfInitialReplyOfTicket = TicketsRepository.Get(t => t.Id == reply.TicketId, "Replies").FirstOrDefault().Replies.First().UserId;
                    if (reply.UserId == _userId)
                        {
                            yield return TicketsRepository.Get(t => t.Id == reply.TicketId, "Replies").FirstOrDefault();
                        }
                }
            }
            else
            {
                foreach (var reply in replies)
                {
                    yield return TicketsRepository.Get(t=>t.Id == reply.TicketId, "Replies").FirstOrDefault();
                }
            }

        }

        private IEnumerable<TicketResponse> GetTicketsByTitleContent(string searchInput)
        {
            if(_userRole == Roles.Customer.ToString())
            {
                foreach (var item in TicketsRepository.Get(t => t.Title.Contains(searchInput) && t.CustomerId == _userId, "Replies").ConvertToTicketResponseArray(ReplyImageRepository))
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in TicketsRepository.Get(t => t.Title.Contains(searchInput), "Replies").ConvertToTicketResponseArray(ReplyImageRepository))
                {
                    yield return item;
                }
            }

        }


    }
}
