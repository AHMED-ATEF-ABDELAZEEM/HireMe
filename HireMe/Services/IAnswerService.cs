using HireMe.Contracts.Answer.Requests;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IAnswerService
    {
        Task<Result<Answer>> AddAnswerAsync(string employerId, AddAnswerRequest request, CancellationToken cancellationToken = default);
    }

    public class AnswerService : IAnswerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnswerService> _logger;

        public AnswerService(AppDbContext context, ILogger<AnswerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Answer>> AddAnswerAsync(string employerId, AddAnswerRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting answer creation process for user {EmployerId} on question {QuestionId}", employerId, request.QuestionId);

            var question = await _context.Questions
                .Include(q => q.Job)
                .Include(q => q.Answer)
                .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

            if (question is null)
            {
                _logger.LogWarning("Answer creation failed: Question with ID {QuestionId} not found", request.QuestionId);
                return Result.Failure<Answer>(AnswerErrors.QuestionNotFound);
            }

            if (question.Job is null)
            {
                _logger.LogWarning("Answer creation failed: User {EmployerId} is not authorized to answer question {QuestionId}", employerId, request.QuestionId);
                return Result.Failure<Answer>(JobErrors.JobNotFound);
            }

            if (question.Job.EmployerId != employerId)
            {
                _logger.LogWarning("Answer creation failed: User {EmployerId} is not authorized to answer question {QuestionId}", employerId, request.QuestionId);
                return Result.Failure<Answer>(AnswerErrors.UnauthorizedAnswerCreation);
            }

            if (question.Answer is not null)
            {
                _logger.LogWarning("Answer creation failed: Question {QuestionId} has already been answered", request.QuestionId);
                return Result.Failure<Answer>(AnswerErrors.QuestionAlreadyAnswered);
            }

            var answer = new Answer
            {
                AnswerText = request.AnswerText,
                QuestionId = request.QuestionId,
                EmployerId = employerId
            };

            await _context.Set<Answer>().AddAsync(answer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Answer created successfully with ID: {AnswerId} for question {QuestionId}", answer.Id, request.QuestionId);
            return Result.Success(answer);
        }
    }
}
