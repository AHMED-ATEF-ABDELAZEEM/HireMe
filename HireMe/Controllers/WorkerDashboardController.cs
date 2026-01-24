using System.Security.Claims;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers;

[Route("worker/dashboard")]
[ApiController]
[Authorize(Roles = DefaultRoles.Worker)]
public class WorkerDashboardController : ControllerBase
{
    private readonly IWorkerDashboardService _workerDashboardService;

    public WorkerDashboardController(IWorkerDashboardService workerDashboardService)
    {
        _workerDashboardService = workerDashboardService;
    }

    /// <summary>
    /// Get worker dashboard (PRIMARY ENTRY POINT FOR WORKER MOBILE APP)
    /// </summary>
    /// <remarks>
    /// Returns context-aware dashboard data based on worker's job connection status with optimized performance:
    /// 
    /// **Two Dashboard Modes**:
    /// 
    /// **MODE 1: Active Job Connection** (Status = "Active" AND InteractionEndDate not passed)
    /// - Worker is currently employed and CANNOT apply to other jobs
    /// - Available actions: Chat with employer, give feedback, make reports
    /// - Returns: Only activeConnection details (applications and questions are null)
    /// - Performance: Single database query
    /// 
    /// **MODE 2: Available for Work** (No connection OR Status = "CancelledByEmployer" OR "CancelledByWorker")
    /// - Worker is available and CAN apply to jobs
    /// - Available actions: Apply to jobs, ask questions, give feedback, make reports
    /// - Chat: Closed if connection was cancelled
    /// - Returns: Full application and question analytics
    /// - ActiveConnection: Can be present with cancelled status (not null) OR null if never had connection
    /// - Performance: 3 parallel database queries
    /// 
    /// **Application Analytics (MODE 2 only)**:
    /// - Filtered by latest JobConnection date if exists (excludes historical data from previous jobs)
    /// - Statistics calculated at database level using aggregation
    /// - Total: Total applications in current job search cycle
    /// - Pending: Awaiting employer response (status = Applied)
    /// - Rejected: Rejected by employer (status = Rejected)
    /// - Closed: Job was closed by employer (status = JobClosed)
    /// - ChooseAnotherPerson: Employer selected another candidate (status = EmployerChooseAnotherWorker)
    /// - Withdrawn: Cancelled by worker (status = Withdrawn)
    /// 
    /// **Question Analytics (MODE 2 only)**:
    /// - Filtered by latest JobConnection date if exists
    /// - Single query with conditional aggregation for performance
    /// - AnsweredQuestions: Employer responded, job still Published
    /// - UnansweredQuestions: Awaiting employer response, job still Published
    /// 
    /// **Active Connection Details**:
    /// - Returned if any job connection exists (active or cancelled)
    /// - Includes: Job title, employer info, contract end date, days remaining, status
    /// - Status values: "Active", "CancelledByEmployer", "CancelledByWorker"
    /// - Mode determined by status field
    /// 
    /// **Sample Response - MODE 1 (Currently Employed)**:
    /// 
    ///     {
    ///         "applications": null,
    ///         "questions": null,
    ///         "activeConnection": {
    ///             "jobConnectionId": 5,
    ///             "jobTitle": "Senior Software Engineer",
    ///             "personName": "Ahmed Mohamed",
    ///             "personImageProfile": "profile.jpg",
    ///             "contractEndDate": "2026-02-15T10:00:00Z",
    ///             "daysRemaining": 24,
    ///             "status": "Active"
    ///         }
    ///     }
    /// 
    /// **Sample Response - MODE 2 (Available, with cancelled connection)**:
    /// 
    ///     {
    ///         "applications": {
    ///             "total": 8,
    ///             "pending": 5,
    ///             "rejected": 2,
    ///             "closed": 1,
    ///             "chooseAnotherPerson": 0,
    ///             "withdrawn": 1
    ///         },
    ///         "questions": {
    ///             "answeredQuestions": 3,
    ///             "unansweredQuestions": 2
    ///         },
    ///         "activeConnection": {
    ///             "jobConnectionId": 5,
    ///             "jobTitle": "Senior Software Engineer",
    ///             "personName": "Ahmed Mohamed",
    ///             "personImageProfile": "profile.jpg",
    ///             "contractEndDate": "2026-02-15T10:00:00Z",
    ///             "daysRemaining": -5,
    ///             "status": "CancelledByEmployer"
    ///         }
    ///     }
    /// 
    /// **Sample Response - MODE 2 (Available, never had connection)**:
    /// 
    ///     {
    ///         "applications": {
    ///             "total": 8,
    ///             "pending": 5,
    ///             "rejected": 2,
    ///             "closed": 1,
    ///             "chooseAnotherPerson": 0,
    ///             "withdrawn": 1
    ///         },
    ///         "questions": {
    ///             "answeredQuestions": 3,
    ///             "unansweredQuestions": 2
    ///         },
    ///         "activeConnection": null
    ///     }
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Dashboard data returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User is not authorized (not a worker).</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dashboard = await _workerDashboardService.GetWorkerDashboardAsync(workerId!, cancellationToken);
        return dashboard.IsSuccess ? Ok(dashboard.Value) : BadRequest(dashboard.Error);
    }
}
