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

        [HttpPost]
        public async Task<IActionResult> AddAnswer([FromBody] AddAnswerRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.AddAnswerAsync(employerId!, request, cancellationToken);

            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }

        [HttpPut("{answerId}")]
        public async Task<IActionResult> UpdateAnswer([FromRoute] int answerId, [FromBody] UpdateAnswerRequest request, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.UpdateAnswerAsync(employerId!, answerId, request, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpDelete("{answerId}")]
        public async Task<IActionResult> DeleteAnswer([FromRoute] int answerId, CancellationToken cancellationToken)
        {
            var employerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _answerService.DeleteAnswerAsync(employerId!, answerId, cancellationToken);

            return result.IsSuccess ? NoContent() : BadRequest(result.Error);
        }

        [HttpGet("questions/{questionId}/answer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnswerByQuestionId([FromRoute] int questionId, CancellationToken cancellationToken)
        {
            var result = await _answerService.GetAnswerByQuestionIdAsync(questionId, cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
        }
    }
}
