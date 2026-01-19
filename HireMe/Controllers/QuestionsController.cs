using System.Security.Claims;
using HireMe.Contracts.Question.Requests;
using HireMe.SeedingData;
using HireMe.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireMe.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize (Roles = DefaultRoles.Worker)]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        /// <summary>
        /// Adds a new question to a job from a worker.
        /// </summary>
        /// <param name="request">The question details including job ID and question content.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 200 OK if the question is added successfully.</returns>
        /// <remarks>
        /// This endpoint allows workers to ask questions about a specific job posting.
        /// The worker ID is automatically extracted from the authenticated user's token.
        /// Workers can ask multiple questions on the same job.
        /// </remarks>
        /// <response code="200">Question added successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., JobNotFound, JobNotAcceptingQuestions).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
        [HttpPost]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionRequest request, CancellationToken cancellationToken)
        {
            var WorkerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.AddQuestionAsync(WorkerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        /// <summary>
        /// Updates an existing question content.
        /// </summary>
        /// <param name="questionId">The ID of the question to update.</param>
        /// <param name="request">The updated question content.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the question is updated successfully.</returns>
        /// <remarks>
        /// Only the worker who created the question can update it.
        /// The question must exist and belong to the authenticated worker.
        /// Cannot update questions that have already been answered by the employer.
        /// </remarks>
        /// <response code="204">Question updated successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., QuestionNotFound, UnauthorizedQuestionUpdate, QuestionAlreadyAnswered).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
        [HttpPut("{questionId}")]
        public async Task<IActionResult> UpdateQuestion([FromRoute] int questionId, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.UpdateQuestionAsync(workerId!, questionId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Deletes a question.
        /// </summary>
        /// <param name="questionId">The ID of the question to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 204 No Content if the question is deleted successfully.</returns>
        /// <remarks>
        /// Only the worker who created the question can delete it.
        /// The question must exist and belong to the authenticated worker.
        /// This is a soft delete operation, maintaining data integrity.
        /// Cannot delete questions that have already been answered by the employer.
        /// </remarks>
        /// <response code="204">Question deleted successfully.</response>
        /// <response code="400">Invalid request or business rule violation (e.g., QuestionNotFound, UnauthorizedQuestionUpdate, QuestionAlreadyAnswered).</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User is not authorized (not a worker).</response>
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion([FromRoute] int questionId, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.DeleteQuestionAsync(workerId!, questionId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        /// <summary>
        /// Retrieves all questions for a specific job.
        /// </summary>
        /// <param name="jobId">The ID of the job to get questions for.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Returns 200 OK with a list of questions and their answers if available.</returns>
        /// <remarks>
        /// This endpoint is publicly accessible (no authentication required).
        /// Returns all questions asked by workers on the specified job along with employer answers if provided.
        /// Useful for workers to see what questions have been asked before submitting their own.
        /// Only returns non-deleted questions.
        /// </remarks>
        /// <response code="200">Questions retrieved successfully. Returns a list of questions with answers.</response>
        /// <response code="400">Invalid request or JobNotFound.</response>
        [HttpGet("{jobId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllQuestions([FromRoute] int jobId, CancellationToken cancellationToken)
        {
            var result = await _questionService.GetAllQuestionsAsync(jobId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
