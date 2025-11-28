using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HireMe.EmailSettings
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _emailSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<MailSettings> emailSettings, IHttpClientFactory httpClientFactory, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _httpClient = httpClientFactory.CreateClient("EmailApi");
            _logger = logger;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var payload = new
            {
                sender = new { email = _emailSettings.Email, name = _emailSettings.DisplayName },
                to = new[] { new { email } },
                subject,
                htmlContent = htmlMessage
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _emailSettings.ApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
