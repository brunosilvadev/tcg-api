using System.ComponentModel.DataAnnotations;

namespace TcgApi.Data.Models.Requests;

public record LoginRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required] string Password
);
