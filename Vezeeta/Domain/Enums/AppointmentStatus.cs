using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum AppointmentStatus : byte
    {
        Pending,
        Confirmed,
        Completed,
        Cancelled
    }
}
