namespace HireMe.Contracts.WorkerDashboard;

public record ActiveJobConnectionResponse(
    int JobConnectionId,
    string JobTitle,
    string PersonName,
    string? PersonImageProfile,
    DateTime ContractEndDate,
    int DaysRemaining,
    string Status
);
