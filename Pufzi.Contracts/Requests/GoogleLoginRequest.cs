using System.ComponentModel.DataAnnotations;

namespace Pufzi.Contracts.Requests.Auth;

public class GoogleLoginRequest
{
    [Required]
    public required string IdToken { get; set; }
}