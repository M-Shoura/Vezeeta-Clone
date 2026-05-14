using Application.Dtos;

public class DashboardVm
{
    public int DoctorsCount { get; set; }

    public int PatientsCount { get; set; }

    public int ClinicsCount { get; set; }

    public int TodayAppointments { get; set; }

    public List<RecentAppointmentVm> RecentAppointments { get; set; }
}