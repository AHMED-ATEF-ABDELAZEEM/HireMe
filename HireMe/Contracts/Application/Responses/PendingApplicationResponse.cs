namespace HireMe.Contracts.Application;

public record PendingApplicationResponse(
    int Id,
    string? Message,
    DateTime CreatedAt,
    string JobTitle,
    decimal Salary,
    string Governorate
);
