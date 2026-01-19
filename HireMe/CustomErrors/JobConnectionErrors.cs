using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class JobConnectionErrors
    {
        public static Error JobConnectionNotFound = new Error("JobConnectionNotFound", "The specified job connection does not exist.");
        public static Error JobConnectionNotActive = new Error("JobConnectionNotActive", "The job connection is not active and cannot be cancelled.");
        public static Error UnauthorizedCancellation = new Error("UnauthorizedCancellation", "You are not authorized to cancel this job connection.");
        public static Error JobConnectionAlreadyCancelled = new Error("JobConnectionAlreadyCancelled", "The job connection has already been cancelled.");
    }
}
