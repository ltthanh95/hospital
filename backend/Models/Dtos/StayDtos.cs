using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class CreateStayRequest
    {
        [Required]
        public required int PatientId { get; set; }

        [Required]
        public required int RoomId { get; set; }

        [Required]
        public required DateTime CheckIn { get; set; }
    }

    public class StayResponse
    {
        public required int Id { get; set; }
        public required int PatientId { get; set; }
        public required string PatientName { get; set; }
        public required int RoomId { get; set; }
        public required string RoomNumber { get; set; }
        public required DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public required int Nights { get; set; }

        public static StayResponse FromEntity(Stay stay) => new()
        {
            Id = stay.Id,
            PatientId = stay.PatientId,
            PatientName = $"{stay.Patient.FName} {stay.Patient.LName}",
            RoomId = stay.RoomId,
            RoomNumber = stay.Room.RoomNumber,
            CheckIn = stay.CheckIn,
            CheckOut = stay.CheckOut,
            Nights = StayNights.Calculate(stay.CheckIn, stay.CheckOut),
        };
    }

    public static class StayNights
    {
        public static int Calculate(DateTime checkIn, DateTime? checkOut)
        {
            var end = checkOut ?? DateTime.UtcNow;
            var nights = (int)Math.Ceiling((end - checkIn).TotalDays);
            return Math.Max(nights, 1);
        }
    }
}
