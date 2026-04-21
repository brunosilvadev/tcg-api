namespace TcgApi.Data.Models;

public class UserDailyTask
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public DateOnly CompletedDate { get; set; }
}
