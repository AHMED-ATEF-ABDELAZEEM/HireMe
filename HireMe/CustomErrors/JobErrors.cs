using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class JobErrors
    {
        public static Error InvalidGovernorate = new Error("InvalidGovernorate", "The specified governorate does not exist.");
        public static Error JobNotFound = new Error("JobNotFound", "The specified job does not exist.");
        public static Error JobAlreadyClosed = new Error("JobAlreadyClosed", "The job is already closed.");
        public static Error JobNotAcceptingQuestions = new Error("JobNotAcceptingQuestions", "It may be closed or completed");
    }
}