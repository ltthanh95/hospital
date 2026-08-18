using backend.dbContext;
using backend.Models;
using backend.Models.Dtos;
using backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

//Handle service for login and register
namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var usernameTaken = await _db.Users.AnyAsync(u => u.Username == request.Username);
            if (usernameTaken)
            {
                return null;
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
            };

            if (request.Role == Role.PATIENT && request.Patient is not null)
            {
                var admissionDate = DateTime.UtcNow;
                user.Patient = new Patient
                {
                    FName = request.Patient.FName,
                    LName = request.Patient.LName,
                    DoB = request.Patient.DoB,
                    Gender = request.Patient.Gender,
                    Address = request.Patient.Address,
                    Phone = request.Patient.Phone,
                    Email = request.Patient.Email,
                    BloodType = request.Patient.BloodType,
                    EmergencyContact = request.Patient.EmergencyContact,
                    AdmissionDate = admissionDate,
                    status = Status.ADMISSION,
                };
            }

            if (request.Role == Role.DOCTOR && request.Doctor is not null)
            {
                var departmentName = request.Doctor.DepartmentName.Trim();
                var department = await _db.Departments.FirstOrDefaultAsync(d => d.Name == departmentName)
                    ?? new Department { Name = departmentName };

                user.Doctor = new Doctor
                {
                    FName = request.Doctor.FName,
                    LName = request.Doctor.LName,
                    DoB = request.Doctor.DoB,
                    Gender = request.Doctor.Gender,
                    Address = request.Doctor.Address,
                    Phone = request.Doctor.Phone,
                    Email = request.Doctor.Email,
                    LicenseNumber = request.Doctor.LicenseNumber,
                    Specialization = request.Doctor.Specialization,
                    ConsulationFee = request.Doctor.ConsulationFee,
                    status = Status.ACTIVE,
                    Department = department,
                };
            }

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildAuthResponse(user);
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            return BuildAuthResponse(user);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            var (token, expiresAt) = _tokenService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                Username = user.Username,
                Role = user.Role,
            };
        }
    }
}
