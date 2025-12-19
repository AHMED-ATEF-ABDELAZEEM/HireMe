using HireMe.Enums;

namespace HireMe.Consts
{
    public static class WorkDaysRules
    {
        // مجموع كل الأيام المسموح بيها
        public const int AllowedMask =
            (int)(
                WorkDays.Saturday |
                WorkDays.Sunday |
                WorkDays.Monday |
                WorkDays.Tuesday |
                WorkDays.Wednesday |
                WorkDays.Thursday |
                WorkDays.Friday
            );
    }
}
