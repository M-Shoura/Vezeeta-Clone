using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities
{
    public class Prescription : AuditableEntity
    {
        public DateTime PrescriptionDate { get; set; }
        public string? Notes { get; set; }
        public int AppointmentId { get; set; }
        //public int DoctorId { get; set; }
        //public int PatientId { get; set; }
        
        public Appointment Appointment { get; set; } = null!;
        //public DoctorProfile Doctor { get; set; } = null!;
        //public PatientProfile Patient { get; set; } = null!;
        
        public ICollection<PrescriptionItem> Items { get; set; } = new HashSet<PrescriptionItem>();
    }
}
