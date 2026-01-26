using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public static class FeedbackErrors
    {
        public static readonly Error JobConnectionNotFound = 
            new("Feedback.JobConnectionNotFound", "Job connection not found.");

        public static readonly Error InteractionPeriodEnded = 
            new("Feedback.InteractionPeriodEnded", "Cannot submit feedback after the interaction end date.");

        public static readonly Error NotPartOfConnection = 
            new("Feedback.NotPartOfConnection", "You are not part of this job connection.");

        public static readonly Error FeedbackAlreadyExists = 
            new("Feedback.AlreadyExists", "You have already submitted feedback for this job connection.");
    }
}
