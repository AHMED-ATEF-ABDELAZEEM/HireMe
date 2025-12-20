using HireMe.Enums;

namespace HireMe.Consts
{
    public class WorkDaysInArabic
    {
        private static readonly Dictionary<WorkDays, string> DayNamesArabic = new Dictionary<WorkDays, string>
        {
            { WorkDays.Saturday,  "السبت" },
            { WorkDays.Sunday,    "الأحد" },
            { WorkDays.Monday,    "الاثنين" },
            { WorkDays.Tuesday,   "الثلاثاء" },
            { WorkDays.Wednesday, "الأربعاء" },
            { WorkDays.Thursday,  "الخميس" },
            { WorkDays.Friday,    "الجمعة" }
        };
    

        public static IEnumerable<string> GetDays(int value)
        {
            var result = new List<string>();

            foreach (WorkDays day in Enum.GetValues(typeof(WorkDays)))
            {
                if (day == WorkDays.None) continue;

                if ((value & (int)day) != 0)
                {
                    result.Add(DayNamesArabic[day]);
                }
            }

            return result;
        }
    }
}
