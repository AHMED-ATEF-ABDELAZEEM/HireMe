using System.Security.Claims;
using HireMe.Consts;
using HireMe.Contracts.Job.Requests;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HireMe.Controllers
{
    [Route("[controller]")] 
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        /// <summary>
        /// Creates a new job posting
        /// </summary>
        /// <remarks>
        /// This endpoint allows authenticated employers to create a new job posting.
        /// The employer ID is extracted from the JWT token.
        /// 
        /// Sample request:
        /// 
        ///     POST /Jobs
        ///     {
        ///         "jobTitle": "Software Developer",
        ///         "salary": 5000.00,
        ///         "hasAccommodation": true,
        ///         "workDays": 31,
        ///         "governorateId": 1,
        ///         "gender": 0,
        ///         "shiftStartTime": "09:00:00",
        ///         "shiftEndTime": "17:00:00",
        ///         "address": "123 Main Street, Cairo",
        ///         "description": "We are looking for an experienced software developer",
        ///         "experience": "3+ years of experience required"
        ///     }
        ///     
        /// WorkDays is a bitmask (Saturday=1, Sunday=2, Monday=4, Tuesday=8, Wednesday=16, Thursday=32, Friday=64) that min value is 1 and max value is 127.
        /// Gender: 0 = Any, 1 = Male, 2 = Female
        /// to get GovernorateId refer to /Governorates endpoint.
        /// </remarks>
        /// <param name="request">The job creation request containing all job details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="200">Job created successfully</response>
        /// <response code="400">
        /// Bad request - Possible errors:
        /// - InvalidGovernorate: The specified governorate does not exist
        /// - Validation errors: Missing or invalid required fields
        /// </response>
        /// <response code="401">Unauthorized - User must be authenticated</response>
        /// 
        [HttpPost]
        [Authorize (Roles = DefaultRoles.Employer)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateJob([FromBody] JobRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _jobService.CreateJobAsync(employerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Retrieves detailed information about a specific job
        /// </summary>
        /// <remarks>
        /// This endpoint returns comprehensive details about a job posting identified by its ID.
        /// No authentication is required to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET /Jobs/5
        ///     
        /// Sample response:
        /// 
        ///     {
        ///         "id": 5,
        ///         "jobTitle": "Software Developer",
        ///         "salary": 5000.00,
        ///         "hasAccommodation": true,
        ///         "workingDaysPerWeek": 5,
        ///         "workingHoursPerDay": 8,
        ///         "gender": 0,
        ///         "shiftType": 0,
        ///         "shiftStartTime": "09:00:00",
        ///         "shiftEndTime": "17:00:00",
        ///         "address": "123 Main Street, Cairo",
        ///         "description": "We are looking for an experienced software developer",
        ///         "experience": "3+ years of experience required",
        ///         "governorateName": "القاهرة",
        ///         "workingDaysInArabic": ["السبت", "الأحد", "الاثنين", "الثلاثاء", "الأربعاء"]
        ///     }
        ///     
        /// Response Fields:
        /// - id: Unique identifier of the job
        /// - jobTitle: Title/position of the job
        /// - salary: Monthly salary in the local currency
        /// - hasAccommodation: Whether accommodation is provided (true/false)
        /// - workingDaysPerWeek: Number of working days per week
        /// - workingHoursPerDay: Number of working hours per day
        /// - gender: Preferred gender for the position
        ///   * 0 = Any (no preference)
        ///   * 1 = Male
        ///   * 2 = Female
        /// - shiftType: Type of work shift
        ///   * 0 = Morning (starts before 12 PM)
        ///   * 1 = Night (starts at or after 12 PM)
        /// - shiftStartTime: Start time of the work shift (HH:mm:ss format)
        /// - shiftEndTime: End time of the work shift (HH:mm:ss format)
        /// - address: Physical location address of the job
        /// - description: Detailed description of the job responsibilities
        /// - experience: Required experience or qualifications
        /// - governorateName: Name of the governorate/region in Arabic
        /// - workingDaysInArabic: List of working days in Arabic (e.g., ["السبت", "الأحد", ...])
        /// </remarks>
        /// <param name="id">The unique identifier of the job</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="200">Returns the job details with all information including translated fields</response>
        /// <response code="400">Bad request - Possible errors:
        /// - JobNotFound: The specified job ID does not exist
        /// </response>
        [HttpGet("{id}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetJobById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _jobService.GetJobByIdAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }



        /// <summary>
        /// Closes a job posting
        /// </summary>
        /// <remarks>
        /// This endpoint closes an active job posting, preventing further applications.
        /// The job status will be updated to closed.
        /// 
        /// Sample request:
        /// 
        ///     PUT /Jobs/5/close
        ///     
        /// </remarks>
        /// <param name="id">The unique identifier of the job to close</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="204">Job closed successfully</response>
        /// <response code="400">Bad request - Possible errors:
        /// - JobNotFound: The specified job ID does not exist
        /// - JobAlreadyClosed: The job is already closed
        /// </response>
        [HttpPut("{id}/close")]
        [Authorize (Roles = DefaultRoles.Employer)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CloseJob([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _jobService.CloseJobAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Retrieves all job postings
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of all available job postings with summary information.
        /// No authentication is required to access this endpoint.
        /// 
        /// Sample response:
        /// 
        ///     GET /Jobs
        ///     [
        ///         {
        ///             "id": 1,
        ///             "jobTitle": "Software Developer",
        ///             "salary": 5000.00,
        ///             "governorateName": "Cairo",
        ///             "numberOfQuestions": 5,
        ///             "numberOfApplications": 12,
        ///             "workingHoursPerDay": 8,
        ///             "workingDaysPerWeek": 5,
        ///             "createdAt": "2026-01-18T10:30:00Z",
        ///             "isUpdated": false
        ///         }
        ///     ]
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <response code="200">Returns the list of all jobs</response>
        /// <response code="400">Bad request - Unexpected error occurred</response>
        [HttpGet]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllJobs(CancellationToken cancellationToken)
        {
            var result = await _jobService.GetAllJobsAsync(cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        
    }
}
