using System.Security.Claims;
using HireMe.Contracts.Answer.Requests;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;

        public AnswersController(IAnswerService answerService)
        {
            _answerService = answerService;
        }

        /// <summary>
        /// Adds a new answer to a question by the employer.
        /// </summary>
        /// <param name="request">The answer details including question ID and answer text.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 200 OK if the answer is added successfully.</returns>
        /// <remarks>
        /// This endpoint allows employers to answer questions on their job postings.
        /// Only the employer who owns the job can answer questions.
        /// The employer ID is automatically extracted from the authenticated user's token.
        /// Questions can only be answered once.
        /// </remarks>
        /// <response code="200">Answer added successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., QuestionNotFound, JobNotFound, UnauthorizedAnswerCreation, QuestionAlreadyAnswered).</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost]
        public async Task<IActionResult> AddAnswer([FromBody] AddAnswerRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.AddAnswerAsync(employerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Updates an existing answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer to update.</param>
        /// <param name="request">The updated answer text.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the answer is updated successfully.</returns>
        /// <remarks>
        /// Only the employer who created the answer can update it.
        /// The answer must exist and belong to the authenticated employer.
        /// </remarks>
        /// <response code="204">Answer updated successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., AnswerNotFound, UnauthorizedAnswerUpdate).</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPut("{answerId}")]
        public async Task<IActionResult> UpdateAnswer([FromRoute] int answerId, [FromBody] UpdateAnswerRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.UpdateAnswerAsync(employerId!, answerId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Deletes an answer.
        /// </summary>
        /// <param name="answerId">The ID of the answer to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the answer is deleted successfully.</returns>
        /// <remarks>
        /// Only the employer who created the answer can delete it.
        /// The answer must exist and belong to the authenticated employer.
        /// This is a soft delete operation, maintaining data integrity.
        /// </remarks>
        /// <response code="204">Answer deleted successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., AnswerNotFound, UnauthorizedAnswerDelete).</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpDelete("{answerId}")]
        public async Task<IActionResult> DeleteAnswer([FromRoute] int answerId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.DeleteAnswerAsync(employerId!, answerId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Retrieves the answer for a specific question.
        /// </summary>
        /// <param name="questionId">The ID of the question to get the answer for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 200 OK with the answer details if it exists.</returns>
        /// <remarks>
        /// This endpoint is publicly accessible (no authentication required).
        /// Returns the answer provided by the employer for the specified question.
        /// Useful for workers and visitors to see answered questions on job postings.
        /// </remarks>
        /// <response code="200">Answer retrieved successfully.</response>
        /// <response code="400">Invalid request or AnswerNotFound.</response>
        [HttpGet("/questions/{questionId}/answer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnswerByQuestionId([FromRoute] int questionId, CancellationToken cancellationToken)
        {
            var result = await _answerService.GetAnswerByQuestionIdAsync(questionId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
