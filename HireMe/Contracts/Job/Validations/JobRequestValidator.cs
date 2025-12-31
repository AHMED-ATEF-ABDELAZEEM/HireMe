using FluentValidation;
using HireMe.Consts;
using HireMe.Contracts.Job.Requests;
using HireMe.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HireMe.Contracts.Job.Validations
{
    public class JobRequestValidator : AbstractValidator<JobRequest>
    {

        private const int MinShiftHours = 2;
        private const int MaxShiftHours = 14;
        public JobRequestValidator()
        {
            RuleFor(x => x.JobTitle)
                .NotEmpty()
                .MaximumLength(50);

             RuleFor(x => x.Salary)
                .NotEmpty()
                .GreaterThanOrEqualTo(100).WithMessage("the salary must be at least 100")
                .LessThanOrEqualTo(20000).WithMessage("the salary must not exceed 20000");

            
            RuleFor(x => x.WorkDays)
               .InclusiveBetween(1, 127)
               .WithMessage("WorkDays must be between 1 and 127.");
               

            RuleFor(x => x.Gender)
                .IsInEnum()
                .WithMessage("Invalid gender value.");

            RuleFor(x => x.ShiftStartTime)
                .NotEmpty();

            RuleFor(x => x.ShiftEndTime)
                .NotEmpty();

            RuleFor(x => x)
                .Must(HaveValidShiftDuration)
                .WithMessage($"The shift duration must be at least {MinShiftHours} hours and at most {MaxShiftHours} hours.");

            RuleFor(x => x.Address)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.Address));

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Experience)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Experience));

            // Additional rules can be added here as needed
        }


        private bool HaveValidShiftDuration(JobRequest jobRequest)
        {
            DateTime start = new DateTime(1, 1, 1, jobRequest.ShiftStartTime.Hour, jobRequest.ShiftStartTime.Minute, jobRequest.ShiftStartTime.Second);
            DateTime end = new DateTime(1, 1, 1, jobRequest.ShiftEndTime.Hour, jobRequest.ShiftEndTime.Minute, jobRequest.ShiftEndTime.Second);

            if (end <= start)
            {
                end = end.AddDays(1);
            }

            var hours = (end - start).TotalHours;

            return hours >= MinShiftHours && hours <= MaxShiftHours;
        }
    }
}