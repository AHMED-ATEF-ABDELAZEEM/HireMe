using System.Security.Claims;
using System.Security.Cryptography;
using Hangfire;
using HireMe.Authentication;
using HireMe.Cache;
using HireMe.Contracts.Auth.Responses;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.EmailSettings;
using HireMe.Models;
using HireMe.Persistence;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Helpers
{
    public interface IAuthServiceHelper
    {

        Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user);

    }

    public class AuthServiceHelper : IAuthServiceHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IJwtProvider _jwtProvider;
        private readonly int _RefreshTokenExpiryDays = 14;
        private readonly ILogger<AuthServiceHelper> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;
        private readonly IRefreshTokenHelper _refreshTokenHelper;

        public AuthServiceHelper(
            UserManager<ApplicationUser> userManager,
            IJwtProvider jwtProvider,
            ILogger<AuthServiceHelper> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            AppDbContext context,
            IRefreshTokenHelper refreshTokenHelper)
        {
            _userManager = userManager;
            _jwtProvider = jwtProvider;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
            _context = context;
            _refreshTokenHelper = refreshTokenHelper;
        }



        public async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
        {
            _logger.LogInformation("Starting Generate Token For Email : {email}", user.Email);

            var userRoles = await _userManager.GetRolesAsync(user);
            var tokenInformation = _jwtProvider.GenerateToken(user, userRoles);
            _logger.LogInformation("JWT token generated For Email : {email}", user.Email);

            var refreshToken = _refreshTokenHelper.GenerateRefreshToken();
            var refreshTokenExpirationDate = DateTime.UtcNow.AddDays(_RefreshTokenExpiryDays);

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = refreshTokenExpirationDate,
                UserId = user.Id
            };
            await _context.RefreshTokens.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refresh token stored in database for user {Email}", user.Email);

            return new AuthResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Token = tokenInformation.Token,
                ExpireIn = tokenInformation.ExpiresIn * 60,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpirationDate
            };
        }



    }

}
