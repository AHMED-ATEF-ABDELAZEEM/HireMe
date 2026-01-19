using System.Security.Claims;
using HireMe.Contracts.EmployerDashboard.Responses;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [Route("employer/dashboard")]
    [ApiController]
    [Authorize(Roles = DefaultRoles.Employer)]
    public class EmployerDashboardController : ControllerBase
    {
        private readonly IEmployerDashboardService _employerDashboardService;
        private readonly IJobService _jobService;

        public EmployerDashboardController(IEmployerDashboardService employerDashboardService, IJobService jobService)
        {
            _employerDashboardService = employerDashboardService;
            _jobService = jobService;
        }

        /// <summary>
        /// Get analytics for the employer's most recent job
        /// </summary>
        /// <remarks>
        /// Returns comprehensive analytics for the employer's latest created job including:
        /// - Application statistics (total, applied, rejected, withdrawn, accepted at another job)
        /// - Number of unanswered questions
        /// - Active job connection details with worker information
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "jobId": 5,
        ///         "jobStatus": "Published",
        ///         "numberOfApplications": 15,
        ///         "appliedApplications": 8,
        ///         "rejectedApplications": 4,
        ///         "withdrawnApplications": 2,
        ///         "acceptedAtAnotherJobApplications": 1,
        ///         "unansweredQuestions": 3,
        ///         "activeConnection": {
        ///             "jobConnectionId": 12,
        ///             "startedAt": "2026-01-15T10:00:00Z",
        ///             "endsAt": "2026-01-25T10:00:00Z",
        ///             "worker": {
        ///                 "workerId": "abc123",
        ///                 "fullName": "Ahmed Mohamed",
        ///                 "imageProfile": "profile.jpg"
        ///             }
        ///         }
        ///     }
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Analytics returned successfully.</response>
        /// <response code="400">Invalid request or JobNotFound (employer has no jobs).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not an employer).</response>
        [HttpGet("analytics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLatestJobAnalytics(CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get last created job id for this employer
            var lastJobId = await _jobService.GetLastJobIdForEmployerAsync(employerId!, cancellationToken);
            if (!lastJobId.IsSuccess)
            {
                return BadRequest(lastJobId.Error);
            }

            var analytics = await _employerDashboardService.GetJobAnalyticsAsync(employerId!, lastJobId.Value, cancellationToken);
            return analytics.IsSuccess ? Ok(analytics.Value) : BadRequest(analytics.Error);
        }

        /// <summary>
        /// Get analytics for a specific job
        /// </summary>
        /// <remarks>
        /// Returns comprehensive analytics for a specific job including:
        /// - Application statistics (total, applied, rejected, withdrawn, accepted at another job)
        /// - Number of unanswered questions
        /// - Active job connection details with worker information
        /// 
        /// Only the employer who owns the job can access its analytics.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "jobId": 5,
        ///         "jobStatus": "Published",
        ///         "numberOfApplications": 15,
        ///         "appliedApplications": 8,
        ///         "rejectedApplications": 4,
        ///         "withdrawnApplications": 2,
        ///         "acceptedAtAnotherJobApplications": 1,
        ///         "unansweredQuestions": 3,
        ///         "activeConnection": {
        ///             "jobConnectionId": 12,
        ///             "startedAt": "2026-01-15T10:00:00Z",
        ///             "endsAt": "2026-01-25T10:00:00Z",
        ///             "worker": {
        ///                 "workerId": "abc123",
        ///                 "fullName": "Ahmed Mohamed",
        ///                 "imageProfile": "profile.jpg"
        ///             }
        ///         }
        ///     }
        /// </remarks>
        /// <param name="jobId">The job ID to get analytics for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">Analytics returned successfully.</response>
        /// <response code="400">Invalid request or JobNotFound/JobNotOwnedByEmployer.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not an employer).</response>
        [HttpGet("analytics/{jobId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetJobAnalytics([FromRoute] int jobId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var analytics = await _employerDashboardService.GetJobAnalyticsAsync(employerId!, jobId, cancellationToken);
            return analytics.IsSuccess ? Ok(analytics.Value) : BadRequest(analytics.Error);
        }
    }
}
