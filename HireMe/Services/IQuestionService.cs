using HireMe.Contracts.Question.Requests;
using HireMe.CustomErrors;
using HireMe.CustomResult;
using HireMe.Enums;
using HireMe.Models;
using HireMe.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HireMe.Services
{
    public interface IQuestionService
    {
        Task<Result<Question>> AddQuestionAsync(string WorkerId, AddQuestionRequest request, CancellationToken cancellationToken = default);
        Task<Result> UpdateQuestionAsync(string workerId, int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    }

    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestionService> _logger;

        public QuestionService(AppDbContext context, ILogger<QuestionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<Question>> AddQuestionAsync(string WorkerId, AddQuestionRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting question creation process for user {UserId} on job {JobId}", WorkerId, request.JobId);

            var job = await _context.Jobs
                .Where(j => j.Id == request.JobId)
                .Select(j => new { j.Id, j.Status })
                .FirstOrDefaultAsync(cancellationToken);
                
            if (job is null)
            {
                _logger.LogWarning("Question creation failed: Job with ID {JobId} not found", request.JobId);
                return Result.Failure<Question>(JobErrors.JobNotFound);
            }

            if (job.Status != JobStatus.Published)
            {
                _logger.LogWarning("Question creation failed: Job with ID {JobId} is not published (Status: {Status})", request.JobId, job.Status);
                return Result.Failure<Question>(JobErrors.JobNotAcceptingQuestions);
            }

            var question = new Question
            {
                QuestionText = request.QuestionText,
                JobId = request.JobId,
                WorkerId = WorkerId,
            };

            await _context.Questions.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Question created successfully with ID: {QuestionId}", question.Id);
            return Result.Success(question);
        }

        public async Task<Result> UpdateQuestionAsync(string workerId, int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting question update process for question {QuestionId} by user {UserId}", questionId, workerId);

            var question = await _context.Questions
                .Include(q => q.Answer)
                .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

            if (question is null)
            {
                _logger.LogWarning("Question update failed: Question with ID {QuestionId} not found", questionId);
                return Result.Failure(QuestionErrors.QuestionNotFound);
            }

            if (question.WorkerId != workerId)
            {
                _logger.LogWarning("Question update failed: User {UserId} is not authorized to update question {QuestionId}", workerId, questionId);
                return Result.Failure(QuestionErrors.UnauthorizedQuestionUpdate);
            }

            if (question.Answer is not null)
            {
                _logger.LogWarning("Question update failed: Question {QuestionId} has already been answered", questionId);
                return Result.Failure(QuestionErrors.QuestionAlreadyAnswered);
            }

            question.QuestionText = request.QuestionText;
            question.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Question updated successfully with ID: {QuestionId}", question.Id);
            return Result.Success();
        }
    }
}
