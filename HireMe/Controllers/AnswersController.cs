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
    }
}
