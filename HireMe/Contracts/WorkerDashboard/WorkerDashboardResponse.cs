namespace HireMe.Contracts.WorkerDashboard;

public record WorkerDashboardResponse(
    ApplicationStatistics? Applications,
    QuestionStatistics? Questions,
    ActiveJobConnectionResponse? ActiveConnection
);

public record ApplicationStatistics(
    int Total,
    int Pending,
    int Rejected,
    int Closed,
    int ChooseAnotherPerson,
    int Withdrawn
);

public record QuestionStatistics(
    int AnsweredQuestions,
    int UnansweredQuestions
);
