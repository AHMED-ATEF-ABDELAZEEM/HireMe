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
    }
}