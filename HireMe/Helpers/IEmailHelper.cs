using Hangfire;
using HireMe.Authentication;
using HireMe.EmailSettings;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace HireMe.Helpers
{
    public interface IEmailHelper
    {
        Task SendConfirmationEmail(ApplicationUser user, string code);
        Task SendResetPasswordEmail(ApplicationUser user, string code);
    }

    public class EmailHelper : IEmailHelper
    {
        private readonly ILogger<EmailHelper> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;

        public EmailHelper(
            UserManager<ApplicationUser> userManager,
            IJwtProvider jwtProvider,
            ILogger<EmailHelper> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            AppDbContext context,
            IRefreshTokenHelper refreshTokenHelper)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }
        public async Task SendConfirmationEmail(ApplicationUser user, string code)
        {
            //var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var origin = "http://shaghlni.runasp.net";
            var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
                new Dictionary<string, string>()
                {
                { "{{name}}",user.FirstName },
                {"{{action_url}}", $"{origin}/Auth/confirm-email?userId={user.Id}&code={code}"}
                });

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "HireMe : Confirm your email", emailBody));

            await Task.CompletedTask;
        }


        public async Task SendResetPasswordEmail(ApplicationUser user, string code)
        {
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;
            var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
                new Dictionary<string, string>()
                {
                { "{{name}}",user.FirstName },
                {"{{action_url}}", $"{origin}/Auth/forgot-password?email={user.Email}&code={code}"}
                });

            BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(user.Email!, "HireMe : Reset password", emailBody));

            _logger.LogInformation("Reset password email sent to {Email}", user.Email);

            await Task.CompletedTask;
        }
    }

}
