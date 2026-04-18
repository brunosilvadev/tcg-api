using System.ComponentModel.DataAnnotations;

namespace TcgApi.Data.Models.Requests;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MaxLength(50)] string Username,
    [Required, MinLength(8), MaxLength(100)] string Password
);
