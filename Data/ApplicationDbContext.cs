using Microsoft.EntityFrameworkCore;
using CareSynq.Models;

namespace CareSynq.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Clinician> Clinicians { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<SensorMachine> SensorMachines { get; set; }
        public DbSet<SensorData> SensorDataRecords { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient configuration
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Email)
                .IsUnique();

            // Clinician configuration
            modelBuilder.Entity<Clinician>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // SensorMachine configuration
            modelBuilder.Entity<SensorMachine>()
                .HasIndex(s => s.SerialNumber)
                .IsUnique();

            // Appointment relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Clinician)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // SensorData relationships
            modelBuilder.Entity<SensorData>()
                .HasOne(sd => sd.Patient)
                .WithMany(p => p.SensorDataRecords)
                .HasForeignKey(sd => sd.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SensorData>()
                .HasOne(sd => sd.SensorMachine)
                .WithMany(sm => sm.SensorDataRecords)
                .HasForeignKey(sd => sd.SensorMachineId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment relationships
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.SensorData)
                .WithMany(sd => sd.Comments)
                .HasForeignKey(c => c.SensorDataId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Patient)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Clinician)
                .WithMany(cl => cl.Comments)
                .HasForeignKey(c => c.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Patients
            modelBuilder.Entity<Patient>().HasData(
                new Patient
                {
                    PatientId = 1,
                    FirstName = "Sarah",
                    LastName = "Khan",
                    Email = "patient@test.com",
                    PasswordHash = "hashedpassword123", 
                    PhoneNumber = "1234567890",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    CreatedDate = DateTime.Now,
                    IsActive = true
                },
                new Patient
                {
                    PatientId = 2,
                    FirstName = "Michael",
                    LastName = "Green",
                    Email = "michael.green@test.com",
                    PasswordHash = "hashedpassword123",
                    PhoneNumber = "0987654321",
                    DateOfBirth = new DateTime(1970, 8, 22),
                    CreatedDate = DateTime.Now,
                    IsActive = false
                }
            );

            // Seed Clinicians
            modelBuilder.Entity<Clinician>().HasData(
                new Clinician
                {
                    ClinicianId = 1,
                    FirstName = "Paul",
                    LastName = "Smith",
                    Email = "clinician@test.com",
                    PasswordHash = "hashedpassword123",
                    PhoneNumber = "5551234567",
                    Specialization = "Wound Care Specialist",
                    CreatedDate = DateTime.Now,
                    IsActive = true
                }
            );

            // Seed Sensor Machines
            modelBuilder.Entity<SensorMachine>().HasData(
                new SensorMachine
                {
                    SensorMachineId = 1,
                    SerialNumber = "SM-2024-001",
                    ModelName = "Sensore Mat Pro",
                    ManufactureDate = new DateTime(2024, 1, 15),
                    LastCalibrationDate = new DateTime(2024, 10, 1),
                    IsActive = true,
                    AssignedPatientId = 1
                },
                new SensorMachine
                {
                    SensorMachineId = 2,
                    SerialNumber = "SM-2024-002",
                    ModelName = "Sensore Mat Pro",
                    ManufactureDate = new DateTime(2024, 2, 20),
                    LastCalibrationDate = new DateTime(2024, 9, 15),
                    IsActive = true,
                    AssignedPatientId = 2
                }
            );

            // Seed Appointments
            modelBuilder.Entity<Appointment>().HasData(
                new Appointment
                {
                    AppointmentId = 1,
                    PatientId = 1,
                    ClinicianId = 1,
                    AppointmentDate = DateTime.Now.AddDays(7),
                    Notes = "Regular checkup for pressure ulcer prevention",
                    Status = "Scheduled",
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}