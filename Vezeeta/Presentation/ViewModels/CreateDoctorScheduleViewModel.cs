using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels
{
    public class CreateDoctorScheduleViewModel
    {
        [Required]
        public string DoctorId { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue)]
        public int ClinicId { get; set; }

        [Required]
        public System.DayOfWeek Day { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(1, 1440)]
        public int SlotDurationMinutes { get; set; } = 30;
    }
}
