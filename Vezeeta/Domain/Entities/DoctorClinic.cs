using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DoctorClinic : AuditableEntity
    {
        public int DoctorId { get; set; }
        public int ClinicId { get; set; } 
        public decimal ConsultationFee { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DoctorProfile Doctor { get; set; } = null!;
        public Clinic Clinic { get; set; } = null!;
    }
}
