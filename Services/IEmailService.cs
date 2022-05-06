using Entities.configutation;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Services.logs;
using System.Threading.Tasks;

namespace Services
{
    public interface IEmailService
    {
        Task<bool> Send(string recipientName, string recipientEmail, string subject, string emailBody, string userId = "");

        public class SmtpService : IEmailService
        {
            private readonly SmtpConfig _smtpConfig;
            private readonly IErrorLogService _errorLogService;
            public SmtpService(IOptions<SmtpConfig> smtpConfig, IErrorLogService errorLogService)
            {
                _smtpConfig = smtpConfig.Value;
                _errorLogService = errorLogService;
            }
            public Task<bool> Send(string recipientName, string recipientEmail, string subject, string emailBody, string userId = "annonymous method")
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
}
