using System.Security.Claims;
using HireMe.Consts;
using HireMe.Contracts.Job.Requests;
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

        [HttpPost]              
        [Authorize]
        public async Task<IActionResult> CreateJob([FromBody] JobRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _jobService.CreateJobAsync(employerId!, request, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _jobService.GetJobByIdAsync(id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpGet("{Id}/days")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkDaysInArabic([FromRoute] int Id, CancellationToken cancellationToken)
        {
            var result = await _jobService.GetWorkDaysAtJobInArabicAsync(Id, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPut("{id}/close")]
        [AllowAnonymous]
        public async Task<IActionResult> CloseJob([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await _jobService.CloseJobAsync(id, cancellationToken);
            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllJobs(CancellationToken cancellationToken)
        {
            var result = await _jobService.GetAllJobsAsync(cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        
    }
}
