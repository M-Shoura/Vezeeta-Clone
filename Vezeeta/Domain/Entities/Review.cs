using Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities
{
    public class Review : AuditableEntity
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
        public int AppointmentId { get; set; }
        public string DoctorProfileId { get; set; }

        public Appointment Appointment { get; set; }
    }
}
