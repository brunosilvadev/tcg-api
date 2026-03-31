using System.ComponentModel.DataAnnotations;

namespace TcgApi.Models.Requests;

public record JoinWaitlistRequest(
    [Required, EmailAddress, MaxLength(255)] string Email
);
