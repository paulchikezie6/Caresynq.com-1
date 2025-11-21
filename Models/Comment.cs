using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSynq.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int SensorDataId { get; set; }

        public int? PatientId { get; set; }

        public int? ClinicianId { get; set; }

        [Required]
        [StringLength(1000)]
        public string CommentText { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? ParentCommentId { get; set; } // For threaded replies

        // Navigation properties
        [ForeignKey("SensorDataId")]
        public virtual SensorData SensorData { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("ClinicianId")]
        public virtual Clinician Clinician { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment ParentComment { get; set; }
    }
}