using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class ApplicationErrors
    {
        public static Error JobNotFound = new Error("JobNotFound", "The specified job does not exist.");
        public static Error JobNotAcceptingApplications  = new Error("JobNotAcceptingApplications", "Cannot apply to a job that may be closed or completed.");
        public static Error AlreadyApplied = new Error("AlreadyApplied", "You have already applied to this job.");
        public static Error ApplicationNotFound = new Error("ApplicationNotFound", "The specified application does not exist.");
        public static Error CannotUpdateApplication = new Error(
            "CannotUpdateApplication", 
            "You cannot update this application because it has already been processed by the employer."
        );
        public static Error UnauthorizedApplicationUpdate = new Error("UnauthorizedApplicationUpdate", "You are not authorized to update this application.");
    }
}
