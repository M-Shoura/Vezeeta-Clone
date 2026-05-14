using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Doctors
{
    public class DoctorDetailsDto
    {
        // Doctor Info
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public string Specialization { get; set; } = null!;
        public int YearsOfExperience { get; set; }
        public string? Bio { get; set; }
        public string Qualification { get; set; } = null!;
        public bool IsAvailable { get; set; }

        // Reviews
        public IEnumerable<Review> Reviews
            = new List<Review>();

        // Schedules
        public IEnumerable<DoctorSchedule> Schedules
            = new List<DoctorSchedule>();

        // Available Appointment Dates
        public IEnumerable<DateTime> AvailableDates
            = new List<DateTime>();
    }
}
