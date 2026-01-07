using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class QuestionErrors
    {
        public static Error QuestionNotFound = new Error("QuestionNotFound", "The specified question does not exist.");
        public static Error QuestionAlreadyAnswered = new Error("QuestionAlreadyAnswered", "Cannot update or delete question that has already been answered.");
        public static Error UnauthorizedQuestionUpdate = new Error("UnauthorizedQuestionUpdate", "You are not authorized to update this question.");
    }
}
