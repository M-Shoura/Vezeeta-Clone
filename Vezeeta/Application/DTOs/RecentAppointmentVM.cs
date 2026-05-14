namespace Application.Dtos;

public class RecentAppointmentVm
{
    public int Id { get; set; }

    public string PatientName { get; set; } = null!;

    public string DoctorName { get; set; } = null!;

    public string ClinicName { get; set; } = null!;

    public DateTime AppointmentDate { get; set; }

    public string Status { get; set; } = null!;
}