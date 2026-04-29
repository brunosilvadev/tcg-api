namespace TcgApi.Data.Models.Requests;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
