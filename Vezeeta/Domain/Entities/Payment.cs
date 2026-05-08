using Domain.Entities.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; }
    }
}
