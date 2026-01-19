using System.Security.Claims;
using HireMe.Contracts.Application.Requests;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> AddApplication([FromBody] AddApplicationRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.AddApplicationAsync(workerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpPut("{applicationId}")]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> UpdateApplication([FromRoute] int applicationId, [FromBody] UpdateApplicationRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.UpdateApplicationAsync(workerId!, applicationId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPatch("{applicationId}/accept")]
        [Authorize(Roles = DefaultRoles.Employer)]
        public async Task<IActionResult> AcceptApplication([FromRoute] int applicationId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.AcceptApplicationAsync(employerId!, applicationId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPatch("{applicationId}/reject")]
        [Authorize(Roles = DefaultRoles.Employer)]
        public async Task<IActionResult> RejectApplication([FromRoute] int applicationId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.RejectApplicationAsync(employerId!, applicationId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpPatch("{applicationId}/withdraw")]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> WithdrawApplication([FromRoute] int applicationId, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.WithdrawApplicationAsync(workerId!, applicationId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }
    }
}
