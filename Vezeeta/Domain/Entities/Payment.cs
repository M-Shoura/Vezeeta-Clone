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
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime? PaidAt { get; set; }
        // public DateTime? RefundedAt { get; set; }
        public string? TransactionReference { get; set; }
        public string? FailureReason { get; set; }
        public int AppointmentId { get; set; }

        public Appointment Appointment { get; set; } = null!;
    }
}
