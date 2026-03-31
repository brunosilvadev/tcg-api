using System.ComponentModel.DataAnnotations;

namespace TcgApi.Models;

public class WaitlistEntry
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;

    public bool IsNotified { get; set; } = false;
}
