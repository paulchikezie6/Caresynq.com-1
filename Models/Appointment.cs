using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSynq.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int ClinicianId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("ClinicianId")]
        public virtual Clinician Clinician { get; set; }
    }
}