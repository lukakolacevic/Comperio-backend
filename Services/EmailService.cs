using System.Net.Mail;
using System.Net;
using dotInstrukcijeBackend.Models;
using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace dotInstrukcijeBackend.Services
{
    public class EmailService
    {
        private readonly string _apiKey;
        private readonly string _fromAddress;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["SendGrid:ApiKey"];
            _fromAddress = configuration["SendGrid:FromAddress"];
            _fromName = configuration["SendGrid:FromName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string confirmationLink)
        {
            var plainTextContent = $"Please confirm your email by visiting this link: {confirmationLink}";
            var htmlContent = $"<p>Please confirm your email by clicking on the link below:</p><a href='{confirmationLink}'>Confirm Email</a>";
            
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromAddress, _fromName);
            var to = new EmailAddress(toEmail);
            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send email: {response.StatusCode}");
            }
        }
    }
}
