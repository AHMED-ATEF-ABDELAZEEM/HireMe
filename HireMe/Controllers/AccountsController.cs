using System.Security.Claims;
using HireMe.Consts;
using HireMe.Contracts.Account.Requests;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Identity.Client;

namespace HireMe.Controllers
{
    [Route("me")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting(RateLimiters.UserLimit)]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IImageProfileService _imageProfileService;
        private readonly IPasswordService _passwordService;
        public AccountsController(IAccountService accountService, IImageProfileService imageProfileService, IPasswordService passwordService)
        {
            _accountService = accountService;
            _imageProfileService = imageProfileService;
            _passwordService = passwordService;
        }


        [HttpGet("")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _accountService.GetUserProfileAsync(userId!);
            return Ok(result.Value);
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _accountService.UpdateProfileAsync(userId!, request);
            return NoContent();
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _passwordService.ChangePasswordAsync(userId!, request);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);

        }

        [HttpPut("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _passwordService.SetPasswordAsync(userId!, request);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        //[HttpPost("profile-image")]
        //public async Task<IActionResult> UploadProfileImage([FromForm] IFormFile Image, CancellationToken cancellationToken = default)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var result = await _imageProfileService.UploadProfileImageAsync(userId, Image, cancellationToken);

        //    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        //}

        [HttpDelete("profile-image")]
        public async Task<IActionResult> DeleteProfileImage(CancellationToken cancellationToken = default)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _imageProfileService.RemoveProfileImageAsync(userId, cancellationToken);

            return result.IsSuccess ? Ok("Deleted successfully") : BadRequest(result.Error);
        }

    }
}
