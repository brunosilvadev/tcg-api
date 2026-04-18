using System.ComponentModel.DataAnnotations;

namespace TcgApi.Data.Models.Requests;

public record JoinWaitlistRequest(
    [Required, EmailAddress, MaxLength(255)] string Email
);
