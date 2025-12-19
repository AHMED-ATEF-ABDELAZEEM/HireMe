using System.ComponentModel.DataAnnotations.Schema;
using HireMe.Enums;

namespace HireMe.Models
{
    public class Job
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public bool HasAccommodation { get; set; }

        public int WorkingDaysPerWeek { get; set; }
        
        public int WorkingHoursPerDay { get; set; } 

        public PreferredGender Gender { get; set; }

        public ShiftType ShiftType { get; set; }

        public TimeOnly ShiftStartTime { get; set; }
        public TimeOnly ShiftEndTime { get; set; }
        public int WorkDays { get; set; } // bitmask representation

        // optional
        public string? Address { get; set; } 
        public string? Description { get; set; }
        public string? Experience { get; set; }




        [ForeignKey(nameof(Governorate))]
        public int GovernorateId {get;set;}

        public Governorate? Governorate {get;set;}  

    }
}
