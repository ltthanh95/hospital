using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class DepartmentRequest
    {
        [Required]
        public required string Name { get; set; }
    }

    public class DepartmentResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required int DoctorCount { get; set; }
        public required List<string> DoctorNames { get; set; }

        public static DepartmentResponse FromEntity(Department department) => new()
        {
            Id = department.Id,
            Name = department.Name,
            DoctorCount = department.Doctors.Count,
            DoctorNames = department.Doctors.Select(doctor => $"{doctor.FName} {doctor.LName}").ToList(),
        };
    }
}
