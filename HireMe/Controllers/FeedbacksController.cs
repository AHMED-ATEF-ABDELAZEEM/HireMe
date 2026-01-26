using HireMe.Contracts.Feedback;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HireMe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Add feedback for a job connection
        /// </summary>
        /// <param name="request">Feedback details including job connection ID, rating (1-5), and optional message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success or error result</returns>
        /// <remarks>
        /// **Validation Rules:**
        /// - Rating: Must be between 1 and 5 (inclusive)
        /// - Message: Cannot exceed 500 characters (optional)
        /// 
        /// **Possible Errors:**
        /// - **Feedback.JobConnectionNotFound** (Code: Feedback.JobConnectionNotFound): Job connection not found.
        /// - **Feedback.InteractionPeriodEnded** (Code: Feedback.InteractionPeriodEnded): Cannot submit feedback after the interaction end date.
        /// - **Feedback.NotPartOfConnection** (Code: Feedback.NotPartOfConnection): You are not part of this job connection.
        /// - **Feedback.AlreadyExists** (Code: Feedback.AlreadyExists): You have already submitted feedback for this job connection.
        /// </remarks>
        /// <response code="200">Feedback created successfully</response>
        /// <response code="400">Invalid request or business rule violation</response>
        /// <response code="401">User is not authenticated</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddFeedback([FromBody] AddFeedbackRequest request, CancellationToken cancellationToken)
        {
            var fromUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _feedbackService.AddFeedbackAsync(fromUserId, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }
    }
}
