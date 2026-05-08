#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum PaymentStatus : byte
    {
        Pending,
        Completed,
        Failed
        // Refunded,
        // PartiallyRefunded
    }
}