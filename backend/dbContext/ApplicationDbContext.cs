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

            modelBuilder.Entity<Department>()
                .HasMany(department => department.Doctors)
                .WithOne(doctor => doctor.Department)
                .HasForeignKey(doctor => doctor.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
