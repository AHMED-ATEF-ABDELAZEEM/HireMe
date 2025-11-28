using System.Security.Claims;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using HireMe.SeedingData;
using Microsoft.AspNetCore.Identity;

namespace HireMe.Helpers
{
    public interface IUserCreationHelper
    {
        // Task<Result<ApplicationUser>> CreateUserAsync(IEnumerable<Claim> claims);

        Task<Result<ApplicationUser>> CreateUserCoreAsync(ApplicationUser user,UserType userType, string? password = null);
    }

    public class UserCreationHelper : IUserCreationHelper
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<UserCreationHelper> _logger;

        public UserCreationHelper(
            UserManager<ApplicationUser> userManager,
            ILogger<UserCreationHelper> logger,
            AppDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }
        // public async Task<Result<ApplicationUser>> CreateUserAsync(IEnumerable<Claim> claims)
        // {

        //     var user = new ApplicationUser
        //     {
        //         UserName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
        //         Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
        //         FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
        //         LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
        //         EmailConfirmed = true
        //     };

        //     return await CreateUserCoreAsync(user);

        // }

        public async Task<Result<ApplicationUser>> CreateUserCoreAsync(ApplicationUser user,UserType userType, string? password = null)
        {
            _logger.LogInformation("Starting Creating User : {Email}", user.Email);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                IdentityResult addResult = new IdentityResult();
                if (password != null) addResult = await _userManager.CreateAsync(user, password);
                else addResult = await _userManager.CreateAsync(user);

                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                    _logger.LogError("Error creating user: {ErrorMessage}", errors);

                    return Result.Failure<ApplicationUser>(UserError.RegisterFailed);

                }

                _logger.LogInformation("Added User Successfully. Id: {UserId}, Email: {Email}", user.Id, user.Email);

                var roleResult = new IdentityResult();

                if (userType == UserType.Employer)
                {
                    _logger.LogInformation("Assigning Employer Role to User : {Email}", user.Email);
                    roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Employer);
                }
                else if (userType == UserType.Worker)
                {
                    _logger.LogInformation("Assigning Worker Role to User : {Email}", user.Email);
                    roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Worker);
                }

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Error Assigning role to user: {ErrorMessage}", errors);
                    await transaction.RollbackAsync();
                    return Result.Failure<ApplicationUser>(UserError.RegisterFailed);
                }

                _logger.LogInformation("User Registered Successfully With Email : {email}", user.Email);

                await transaction.CommitAsync();

                return Result.Success(user);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user: {ErrorMessage}", ex.Message);
                await transaction.RollbackAsync();
                return Result.Failure<ApplicationUser>(UserError.RegisterFailed);
            }
        }
    }



}
