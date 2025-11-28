using System.Security.Claims;
using System.Text;
using HireMe.Authentication;
using HireMe.Cache;
using HireMe.Contracts.Account.Requests;
using HireMe.Contracts.Auth.Requests;
using HireMe.Contracts.Auth.Responses;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.EmailSettings;
using HireMe.Helpers;
using HireMe.Models;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);

        Task<Result<AuthResponse>> GoogleLoginAsync(HttpContext httpContext);

    }

    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly ITemporarySessionStore _temporarySessionStore;
        private readonly IAuthServiceHelper _authServiceHelper;
        private readonly IUserCreationHelper _userCreationHelper;
        public AuthService(UserManager<ApplicationUser> userManager,
            ILogger<AuthService> logger,
            SignInManager<ApplicationUser> signInManager,
            ITemporarySessionStore temporarySessionStore,
            IAuthServiceHelper authServiceHelper,
            IUserCreationHelper userCreationHelper)
        {
            _userManager = userManager;
            _logger = logger;
            _signInManager = signInManager;
            _temporarySessionStore = temporarySessionStore;
            _authServiceHelper = authServiceHelper;
            _userCreationHelper = userCreationHelper;
        }

        public async Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
        {

            _logger.LogInformation("Starting Login process for user with email {Email}", email);

            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} not found", email);
                return Result.Failure<AuthResponse>(UserError.InvalidCredentials);
            }

            if (user.PasswordHash is null)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} has no password (External Login)", email);
                return Result.Failure<AuthResponse>(UserError.ExternalLogin);
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} is not confirmed", email);
                return Result.Failure<AuthResponse>(UserError.EmailNotConfirmed);
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Authentication failed: User with email {Email} is locked out", email);
                return Result.Failure<AuthResponse>(UserError.LockedOut);
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("Authentication failed: Invalid password for user with email {Email}", email);
                return Result.Failure<AuthResponse>(UserError.InvalidCredentials);
            }

            var authResponse = await _authServiceHelper.GenerateAuthResponseAsync(user);

            _logger.LogInformation("Authentication Success for user with email {Email}", email);

            return Result.Success(authResponse);
        }

        public async Task<Result<AuthResponse>> GoogleLoginAsync(HttpContext httpContext)
        {
            var result = await httpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                _logger.LogWarning("External authentication failed.");
                return Result.Failure<AuthResponse>(ExternalAuthError.AuthenticationFailed);
            }


            var claims = result.Principal!.Claims;

            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;


            var emailVerifiedClaim = claims.FirstOrDefault(c => c.Type == "email_verified")?.Value;

            bool.TryParse(emailVerifiedClaim, out bool emailVerified);

            if (email is null)
            {
                _logger.LogWarning("External authentication failed: user email not found in claims.");
                return Result.Failure<AuthResponse>(ExternalAuthError.UserEmailNotFound);
            }

            if (!emailVerified)
            {
                _logger.LogWarning("External authentication failed: user email not verified.");
                return Result.Failure<AuthResponse>(ExternalAuthError.UserEmailNotVerified);
            }

            _logger.LogInformation("Starting External Login Using Google For Email : {email}", email);

            var user = await _userManager.FindByEmailAsync(email!);

            // if (user is null)
            // {
            //     var createResult = await _userCreationHelper.CreateUserAsync(claims);

            //     if (createResult.IsSuccess)
            //     {
            //         user = createResult.Value;
            //     }
            //     else
            //     {
            //         return Result.Failure<AuthResponse>(createResult.Error);
            //     }

            // }

            var authResponse = await _authServiceHelper.GenerateAuthResponseAsync(user);

            return Result.Success(authResponse);

        }



    }
}
