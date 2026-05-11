using Domain.Entities.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Appointment : AuditableEntity
    {
        public DateTime AppointmentDate { get; set; } 
        public TimeSpan StartTime { get; set; }       
        public TimeSpan EndTime { get; set; }         
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Notes { get; set; }

        public int? DoctorId { get; set; } = null!;
        public int? PatientId { get; set; } = null!;

        public int ClinicId { get; set; } 
        public int PaymentId { get; set; }
        public int PrescriptionId { get; set; }
        public int ReviewId { get; set; }

        public DoctorProfile Doctor { get; set; } = null!;
        public PatientProfile Patient { get; set; } = null!; 
        public Clinic Clinic { get; set; } = null!;

        public Review? Review { get; set; } 
        public Payment? Payment { get; set; } 
        public Prescription? Prescription { get; set; } 
    }
}
