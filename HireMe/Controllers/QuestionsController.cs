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

        [HttpPost]
        [Authorize(Roles = DefaultRoles.Worker)]
        public async Task<IActionResult> AddQuestion([FromBody] AddQuestionRequest request, CancellationToken cancellationToken)
        {
            var WorkerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.AddQuestionAsync(WorkerId!, request, cancellationToken);

            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpPut("{questionId}")]
        public async Task<IActionResult> UpdateQuestion([FromRoute] int questionId, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.UpdateQuestionAsync(workerId!, questionId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion([FromRoute] int questionId, CancellationToken cancellationToken)
        {
            var workerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _questionService.DeleteQuestionAsync(workerId!, questionId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpGet("{jobId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllQuestions([FromRoute] int jobId, CancellationToken cancellationToken)
        {
            var result = await _questionService.GetAllQuestionsAsync(jobId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
