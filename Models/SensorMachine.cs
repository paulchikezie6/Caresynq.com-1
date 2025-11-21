using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CareSynq.Models
{
    public class SensorMachine
    {
        [Key]
        public int SensorMachineId { get; set; }

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; }

        [StringLength(100)]
        public string ModelName { get; set; } = "Sensore Mat";

        public DateTime ManufactureDate { get; set; }

        public DateTime LastCalibrationDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int? AssignedPatientId { get; set; }

        // Navigation properties
        [ForeignKey("AssignedPatientId")]
        public virtual Patient AssignedPatient { get; set; }

        public virtual ICollection<SensorData> SensorDataRecords { get; set; }
    }
}
