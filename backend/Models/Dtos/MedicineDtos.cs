using System.ComponentModel.DataAnnotations;

namespace backend.Models.Dtos
{
    public class MedicineRequest
    {
        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Manufacturer { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQt { get; set; }

        [Required]
        public required DateTime Expiration { get; set; }
    }

    public class UpdateMedicineStockRequest
    {
        [Range(0, int.MaxValue)]
        public int StockQt { get; set; }
    }

    public class MedicineResponse
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string Manufacturer { get; set; }
        public required decimal UnitPrice { get; set; }
        public required int StockQt { get; set; }
        public required DateTime Expiration { get; set; }

        public static MedicineResponse FromEntity(Medicine medicine) => new()
        {
            Id = medicine.Id,
            Name = medicine.Name,
            Manufacturer = medicine.Manufacturer,
            UnitPrice = medicine.UnitPrice,
            StockQt = medicine.StockQt,
            Expiration = medicine.Expiration,
        };
    }
}
