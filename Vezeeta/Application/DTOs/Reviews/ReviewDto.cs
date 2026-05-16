namespace Application.DTOs.Reviews
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int AppointmentId { get; set; }
        public string DoctorId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string DoctorName { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
