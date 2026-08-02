using backend.Models.Abstraction;

namespace backend.Models
{
    public class Patient : Person
    {

        public string BloodType { get; set; }
        public DateTime AdmissionDate { get; set; }

        public DateTime? DischargeDate { get; set; }

        public Status status { get; set; }

        public string EmergencyContact { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public void Admission()
        {
            status = Status.ADMISSION;
        }
        public void Discharge()
        {
            status = Status.DISCHARGE;
        }
    }
}
