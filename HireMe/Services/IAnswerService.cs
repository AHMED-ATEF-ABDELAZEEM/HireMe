using HireMe.Contracts.Answer.Requests;
using HireMe.Contracts.Answer.Responses;
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
        Task<Result> UpdateAnswerAsync(string employerId, int answerId, UpdateAnswerRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeleteAnswerAsync(string employerId, int answerId, CancellationToken cancellationToken = default);
        Task<Result<AnswerSummaryResponse>> GetAnswerByQuestionIdAsync(int questionId, CancellationToken cancellationToken = default);
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


        // TODO : send notification to worker when their question is answered
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

            await _context.Answers.AddAsync(answer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Answer created successfully with ID: {AnswerId} for question {QuestionId}", answer.Id, request.QuestionId);
            return Result.Success(answer);
        }

        public async Task<Result> UpdateAnswerAsync(string employerId, int answerId, UpdateAnswerRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting answer update process for answer {AnswerId} by user {EmployerId}", answerId, employerId);

            var answer = await _context.Answers
                .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

            if (answer is null)
            {
                _logger.LogWarning("Answer update failed: Answer with ID {AnswerId} not found", answerId);
                return Result.Failure(AnswerErrors.AnswerNotFound);
            }

            if (answer.EmployerId != employerId)
            {
                _logger.LogWarning("Answer update failed: User {EmployerId} is not authorized to update answer {AnswerId}", employerId, answerId);
                return Result.Failure(AnswerErrors.UnauthorizedAnswerUpdate);
            }

            answer.AnswerText = request.AnswerText;
            answer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Answer updated successfully with ID: {AnswerId}", answer.Id);
            return Result.Success();
        }

        public async Task<Result> DeleteAnswerAsync(string employerId, int answerId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting answer deletion process for answer {AnswerId} by user {EmployerId}", answerId, employerId);

            var answer = await _context.Answers
                .FirstOrDefaultAsync(a => a.Id == answerId, cancellationToken);

            if (answer is null)
            {
                _logger.LogWarning("Answer deletion failed: Answer with ID {AnswerId} not found", answerId);
                return Result.Failure(AnswerErrors.AnswerNotFound);
            }

            if (answer.EmployerId != employerId)
            {
                _logger.LogWarning("Answer deletion failed: User {EmployerId} is not authorized to delete answer {AnswerId}", employerId, answerId);
                return Result.Failure(AnswerErrors.UnauthorizedAnswerDelete);
            }

            answer.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Answer deleted successfully with ID: {AnswerId}", answer.Id);
            return Result.Success();
        }

        public async Task<Result<AnswerSummaryResponse>> GetAnswerByQuestionIdAsync(int questionId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving answer for question ID: {QuestionId}", questionId);

            var answer = await _context.Answers
                .Where(a => a.QuestionId == questionId)
                .Select(a => new AnswerSummaryResponse
                {
                    Id = a.Id,
                    Text = a.AnswerText,
                    QuestionId = a.QuestionId,
                    CreatedAt = a.CreatedAt,
                    IsUpdated = a.UpdatedAt != null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (answer is null)
            {
                _logger.LogWarning("Answer not found for question ID: {QuestionId}", questionId);
                return Result.Failure<AnswerSummaryResponse>(AnswerErrors.AnswerNotFound);
            }

            _logger.LogInformation("Successfully retrieved answer for question ID: {QuestionId}", questionId);
            return Result.Success(answer);
        }
    }
}
