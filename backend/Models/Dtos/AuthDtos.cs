using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models.Dtos
{
    public class RegisterRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [Required]
        public required Role Role { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }
    }

    public class AuthResponse
    {
        // Carried internally so the controller can set the auth cookie; never serialized to the client.
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
        public required DateTime ExpiresAt { get; set; }
        public required string Username { get; set; }
        public required Role Role { get; set; }
    }
}
