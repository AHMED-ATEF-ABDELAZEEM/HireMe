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

        /// <summary>
        /// Submits a new application to a job posting from a worker.
        /// </summary>
        /// <param name="request">The application details including job ID and message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 200 OK if the application is submitted successfully.</returns>
        /// <remarks>
        /// This endpoint allows workers to apply for job postings.
        /// The worker ID is automatically extracted from the authenticated user's token.
        /// Workers can only apply once to each job.
        /// Jobs must be in Published status to accept applications.
        /// The application includes a message from the worker to the employer.
        /// </remarks>
        /// <response code="200">Application submitted successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., JobNotFound, JobNotAcceptingApplications, AlreadyApplied).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
        [HttpPost]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> AddApplication([FromBody] AddApplicationRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.AddApplicationAsync(workerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Updates an existing application message.
        /// </summary>
        /// <param name="applicationId">The ID of the application to update.</param>
        /// <param name="request">The updated application message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the application is updated successfully.</returns>
        /// <remarks>
        /// Only the worker who submitted the application can update it.
        /// The application must be in Applied status (not yet processed by employer).
        /// Cannot update applications that have been accepted, rejected, or withdrawn.
        /// </remarks>
        /// <response code="204">Application updated successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., ApplicationNotFound, UnauthorizedApplicationUpdate, CannotUpdateApplication).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
        [HttpPut("{applicationId}")]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> UpdateApplication([FromRoute] int applicationId, [FromBody] UpdateApplicationRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.UpdateApplicationAsync(workerId!, applicationId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Accepts a worker's application and creates a job connection.
        /// </summary>
        /// <param name="applicationId">The ID of the application to accept.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the application is accepted successfully.</returns>
        /// <remarks>
        /// Only the employer who owns the job can accept applications.
        /// The application must be in Applied status.
        /// Creates a JobConnection with 10-day interaction period.
        /// Automatically rejects other applications for the same job.
        /// Updates the job status to InProgress.
        /// Worker cannot have another active job connection.
        /// </remarks>
        /// <response code="204">Application accepted successfully and job connection created.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., ApplicationNotFound, JobNotOwnedByEmployer, JobNotAcceptingApplications, InvalidApplicationStatus, WorkerHasActiveConnection).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not an employer).</response>
        [HttpPatch("{applicationId}/accept")]
        [Authorize(Roles = DefaultRoles.Employer)]
        public async Task<IActionResult> AcceptApplication([FromRoute] int applicationId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.AcceptApplicationAsync(employerId!, applicationId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Rejects a worker's application.
        /// </summary>
        /// <param name="applicationId">The ID of the application to reject.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the application is rejected successfully.</returns>
        /// <remarks>
        /// Only the employer who owns the job can reject applications.
        /// The application must be in Applied status.
        /// Once rejected, the application cannot be updated by the worker.
        /// </remarks>
        /// <response code="204">Application rejected successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., ApplicationNotFound, InvalidApplicationStatus, JobNotOwnedByEmployer).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not an employer).</response>
        [HttpPatch("{applicationId}/reject")]
        [Authorize(Roles = DefaultRoles.Employer)]
        public async Task<IActionResult> RejectApplication([FromRoute] int applicationId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _applicationService.RejectApplicationAsync(employerId!, applicationId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Withdraws a worker's application.
        /// </summary>
        /// <param name="applicationId">The ID of the application to withdraw.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the application is withdrawn successfully.</returns>
        /// <remarks>
        /// Only the worker who submitted the application can withdraw it.
        /// The application must be in Applied status (not yet processed by employer).
        /// Cannot withdraw applications that have been accepted or rejected.
        /// Once withdrawn, the application cannot be reactivated.
        /// </remarks>
        /// <response code="204">Application withdrawn successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., ApplicationNotFound, UnauthorizedApplicationUpdate, CannotUpdateApplication).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
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
