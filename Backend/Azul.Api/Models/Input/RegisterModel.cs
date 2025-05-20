using System.ComponentModel.DataAnnotations;

namespace Azul.Api.Models.Input;

public class RegisterModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public DateOnly? LastVisitToPortugal { get; set; }
}