using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace CareSynq.Models
{
    public class SensorData
    {
        [Key]
        public int SensorDataId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int SensorMachineId { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string PressureMapData { get; set; } // JSON string of 32x32 matrix

        public double PeakPressureIndex { get; set; }

        public double ContactAreaPercentage { get; set; }

        public bool IsHighPressureAlert { get; set; } = false;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("SensorMachineId")]
        public virtual SensorMachine SensorMachine { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}
