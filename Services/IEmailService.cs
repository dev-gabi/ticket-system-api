using Entities.configutation;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.logs;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IEmailService
    {
        Task<bool> Send(string recipientName, string recipientEmail, string subject, string emailBody, string userId = "");
        Task<bool> SendConfirmationEmailAsync(IdentityUser identityUser);
    }

    public class SmtpService : IEmailService
    {
        private readonly SmtpConfig _smtpConfig;
        private readonly IErrorLogService _errorLogService;
        private readonly DomainConfig _domainConfig;
        private UserManager<IdentityUser> _userManager;


        public SmtpService(IOptions<SmtpConfig> smtpConfig, IErrorLogService errorLogService, UserManager<IdentityUser> userManager,
            IOptions<DomainConfig> domainConfig)
        {
            _smtpConfig = smtpConfig.Value;
            _errorLogService = errorLogService;
            _userManager = userManager;
            _domainConfig = domainConfig.Value;
        }

        public Task<bool> Send(string recipientName, string recipientEmail, string subject, string emailBody, string userId = "annonymous method - no user is logged in")
        {
            try
            {
                return Task.Run(() =>
                {
                    var emailMessage = new MimeKit.MimeMessage();

                    emailMessage.From.Add(new MailboxAddress(_smtpConfig.From, _smtpConfig.EmailAddress));

                    emailMessage.To.Add(new MailboxAddress(recipientName, recipientEmail));
                    emailMessage.Subject = subject;
                    var builder = new BodyBuilder();
                    builder.HtmlBody = emailBody;
                    emailMessage.Body = builder.ToMessageBody();
                    return IsSent(emailMessage);
                });
            }
            catch (System.Exception x)
            {
                _errorLogService.LogError($"SmtpService - Send: {x.Message} {x.InnerException}", userId);
                return Task.FromResult(false);
            }

        }


        public async Task<bool> SendConfirmationEmailAsync(IdentityUser identityUser)
        {
            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
            var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

            string url = $"{_domainConfig.BackEnd}/api/auth/confirm-email?userid={identityUser.Id}&token={validEmailToken}";
            return await Send(identityUser.UserName, identityUser.Email, "Ticket System Registration Confirmation", $"<h1>Welcome to Support Ticket System</h1>" +
               $"<p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");
        }

        bool IsSent(MimeMessage emailMessage)
        {
            Task connectAndSend = ConnectToClientAndSend(emailMessage);
            connectAndSend.Wait();
            bool isSent = connectAndSend.IsCompletedSuccessfully;
            if (isSent) { return true; }
            else { return false; }
        }
        Task<bool> ConnectToClientAndSend(MimeMessage emailMessage)
        {
            Task.Run(() =>
            {
                using (var client = new SmtpClient())
                {
                    client.Connect(_smtpConfig.Smtp, int.Parse(_smtpConfig.Port));

                    client.Authenticate(_smtpConfig.EmailAddress, _smtpConfig.EmailPass);
                    client.Send(emailMessage);
                    client.Disconnect(true);
                    return true;
                }
            });
            return Task.FromResult(false);
        }
    }
}
