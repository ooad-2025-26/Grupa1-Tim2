// Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ETFTalentProgram.Models;

namespace ETFTalentProgram.Services
{
    public interface IEmailService
    {
        Task PosaljiAsync(string primaocEmail, string naslov, string htmlSadrzaj, string posiljaocEmail);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task PosaljiAsync(string primaocEmail, string naslov, string htmlSadrzaj, string posiljaocEmail)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_settings.ImePosiljaoca, posiljaocEmail));
            email.To.Add(MailboxAddress.Parse(primaocEmail));
            email.Subject = naslov;

            email.Body = new BodyBuilder
            {
                HtmlBody = htmlSadrzaj
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            smtp.ServerCertificateValidationCallback =
    (s, cert, chain, errors) => true;
            await smtp.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.KorisnickoIme, _settings.Lozinka);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}