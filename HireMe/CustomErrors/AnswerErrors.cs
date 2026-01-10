using HireMe.CustomResult;

namespace HireMe.CustomErrors
{
    public class AnswerErrors
    {
        public static Error QuestionNotFound = new Error("QuestionNotFound", "The specified question does not exist.");
        public static Error QuestionAlreadyAnswered = new Error("QuestionAlreadyAnswered", "This question has already been answered.");
        public static Error UnauthorizedAnswerCreation = new Error("UnauthorizedAnswerCreation", "You are not authorized to answer this question.");
        public static Error UnauthorizedAnswerUpdate = new Error("UnauthorizedAnswerUpdate", "You are not authorized to update this answer.");
        public static Error UnauthorizedAnswerDelete = new Error("UnauthorizedAnswerDelete", "You are not authorized to delete this answer.");
        public static Error AnswerNotFound = new Error("AnswerNotFound", "The specified answer does not exist.");
    }
}
