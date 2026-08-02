namespace backend.Models.Dtos
{
    public class RevenueResponse
    {
        public required decimal TotalPatientPayments { get; set; }
        public required decimal TotalDoctorSalaries { get; set; }
        public required decimal NetRevenue { get; set; }
    }
}
