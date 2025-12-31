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
        Task<Result<Question>> AddQuestionAsync(string userId, int jobId, AddQuestionRequest request, CancellationToken cancellationToken = default);
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

        public async Task<Result<Question>> AddQuestionAsync(string userId, int jobId, AddQuestionRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting question creation process for user {UserId} on job {JobId}", userId, jobId);

            var job = await _context.Jobs
                .Where(j => j.Id == jobId)
                .Select(j => new { j.Id, j.Status })
                .FirstOrDefaultAsync(cancellationToken);
                
            if (job is null)
            {
                _logger.LogWarning("Question creation failed: Job with ID {JobId} not found", jobId);
                return Result.Failure<Question>(JobErrors.JobNotFound);
            }

            if (job.Status != JobStatus.published)
            {
                _logger.LogWarning("Question creation failed: Job with ID {JobId} is not published (Status: {Status})", jobId, job.Status);
                return Result.Failure<Question>(JobErrors.JobNotAcceptingQuestions);
            }

            var question = new Question
            {
                QuestionText = request.QuestionText,
                JobId = jobId,
                WorkerId = userId,
            };

            await _context.Questions.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Question created successfully with ID: {QuestionId}", question.Id);
            return Result.Success(question);
        }
    }
}
