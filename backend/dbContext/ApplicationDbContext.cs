using backend.Models;
using backend.Models.Abstraction;
using Microsoft.EntityFrameworkCore;
namespace backend.dbContext
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Department> Departments { get; set; }

        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Stay> Stays { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(user => user.Patient)
                .WithOne(patient => patient.User)
                .HasForeignKey<Patient>(patient => patient.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
               .HasOne(user => user.Doctor)
               .WithOne(doctor => doctor.User)
               .HasForeignKey<Doctor>(doctor => doctor.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .Property(doctor => doctor.ConsulationFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Medicine>()
                .Property(medicine => medicine.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(invoice => invoice.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(item => item.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(payment => payment.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Department>()
                .HasMany(department => department.Doctors)
                .WithOne(doctor => doctor.Department)
                .HasForeignKey(doctor => doctor.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Appointment>()
                .HasOne(appointment => appointment.Doctor)
                .WithMany(doctor => doctor.Appointment)
                .HasForeignKey(appointment => appointment.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(appointment => appointment.Patient)
                .WithMany(patient => patient.Appointment)
                .HasForeignKey(appointment => appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(record => record.Doctor)
                .WithMany()
                .HasForeignKey(record => record.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(record => record.Patient)
                .WithMany(patient => patient.MedicalRecords)
                .HasForeignKey(record => record.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionItem>()
                .HasOne(item => item.Medicine)
                .WithMany(medicine => medicine.PrescriptionItems)
                .HasForeignKey(item => item.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(invoice => invoice.Patient)
                .WithMany()
                .HasForeignKey(invoice => invoice.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Invoice)
                .WithMany(invoice => invoice.Payments)
                .HasForeignKey(payment => payment.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Patient)
                .WithMany(patient => patient.Payment)
                .HasForeignKey(payment => payment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stay>()
                .HasOne(stay => stay.Patient)
                .WithMany()
                .HasForeignKey(stay => stay.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stay>()
                .HasOne(stay => stay.Room)
                .WithMany()
                .HasForeignKey(stay => stay.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
