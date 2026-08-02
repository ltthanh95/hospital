using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class RoomRequest
    {
        [Required]
        public required string RoomNumber { get; set; }

        [Required]
        public required string Type { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }

    public class RoomResponse
    {
        public required int Id { get; set; }
        public required string RoomNumber { get; set; }
        public required string Type { get; set; }
        public required int Capacity { get; set; }
        public required int CurrentOccupancy { get; set; }

        public static RoomResponse FromEntity(Room room) => new()
        {
            Id = room.Id,
            RoomNumber = room.RoomNumber,
            Type = room.Type,
            Capacity = room.Capacity,
            CurrentOccupancy = room.Stays.Count(stay => stay.CheckOut == null),
        };
    }
}
