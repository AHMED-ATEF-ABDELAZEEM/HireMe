namespace HireMe.Enums
{
    public enum JobStatus
    {
        Published = 0,
        InProgress = 1,
        Completed = 2,
        Closed = 3,
        Cancelled = 4

    }

    public enum ApplicationStatus
    {
        Applied = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3,
        WorkerAcceptedAtAnotherJob = 4,
        EmployerChooseAnotherWorker = 5,
        JobClosed = 6
    }

    public enum JobConnectionStatus
    {
        Active = 0,      
        Completed = 1,  
        CancelledByWorker = 2,
        CancelledByEmployer = 3
    }
}   