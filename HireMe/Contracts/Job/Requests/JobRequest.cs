using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HireMe.Enums;

namespace HireMe.Contracts.Job.Requests
{
    public class JobRequest
    {
        public string JobTitle { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool HasAccommodation { get; set; }
        public int WorkDays { get; set; } // bitmask representation
        public int GovernorateId { get; set; }
        public PreferredGender Gender { get; set; }
        public TimeOnly ShiftStartTime { get; set; }
        public TimeOnly ShiftEndTime { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Experience { get; set; }
    }
}