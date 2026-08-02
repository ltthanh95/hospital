using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.Models.Dtos
{
    public class RegisterRequest : IValidatableObject
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [Required]
        public required Role Role { get; set; }

        public PatientRegistrationDetails? Patient { get; set; }

        public DoctorRegistrationDetails? Doctor { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role == Role.PATIENT && Patient is null)
            {
                yield return new ValidationResult(
                    "Patient details are required when registering a PATIENT account.",
                    [nameof(Patient)]);
            }

            if (Role == Role.DOCTOR && Doctor is null)
            {
                yield return new ValidationResult(
                    "Doctor details are required when registering a DOCTOR account.",
                    [nameof(Doctor)]);
            }
        }
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
        [JsonIgnore]
        public string Token { get; set; } = string.Empty;
        public required DateTime ExpiresAt { get; set; }
        public required string Username { get; set; }
        public required Role Role { get; set; }
    }
}
