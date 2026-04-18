using System.ComponentModel.DataAnnotations;

namespace TcgApi.Data.Models.Requests;

public record RefreshTokenRequest(
    [Required] string RefreshToken
);
