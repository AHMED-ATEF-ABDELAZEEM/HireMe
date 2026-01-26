using HireMe.Enums;

namespace HireMe.Contracts.Job.Responses
{
    public class JobResponse
    {
        public int Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public bool HasAccommodation { get; set; }
        public int WorkingDaysPerWeek { get; set; }
        public int WorkingHoursPerDay { get; set; }
        public PreferredGender Gender { get; set; }
        public ShiftType ShiftType { get; set; }
        public TimeOnly ShiftStartTime { get; set; }
        public TimeOnly ShiftEndTime { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Experience { get; set; }
        public string GovernorateName { get; set; } = string.Empty;
        public IEnumerable<string> WorkingDaysInArabic { get; set; } = new List<string>();
    }
}
