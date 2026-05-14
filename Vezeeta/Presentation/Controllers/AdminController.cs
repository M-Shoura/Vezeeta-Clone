using Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "Admin")]    
        public IActionResult Index()
        {
            DashboardVm vm = new()
            {
                DoctorsCount = 125,

                PatientsCount = 2480,

                ClinicsCount = 18,

                TodayAppointments = 64,

                RecentAppointments = new List<RecentAppointmentVm>
                    {
                        new()
                        {
                            Id = 1,
                            PatientName = "Ahmed Mohamed",
                            DoctorName = "Dr. Ali Hassan",
                            ClinicName = "Dental Clinic",
                            AppointmentDate = DateTime.Now.AddHours(2),
                            Status = "Confirmed"
                        },

                        new()
                        {
                            Id = 2,
                            PatientName = "Sara Ahmed",
                            DoctorName = "Dr. Omar Khaled",
                            ClinicName = "Cardiology Clinic",
                            AppointmentDate = DateTime.Now.AddHours(4),
                            Status = "Pending"
                        },

                        new()
                        {
                            Id = 3,
                            PatientName = "Mohamed Adel",
                            DoctorName = "Dr. Mostafa Samir",
                            ClinicName = "Neurology Clinic",
                            AppointmentDate = DateTime.Now.AddDays(1),
                            Status = "Cancelled"
                        },

                        new()
                        {
                            Id = 4,
                            PatientName = "Mariam Tarek",
                            DoctorName = "Dr. Youssef Ahmed",
                            ClinicName = "Orthopedic Clinic",
                            AppointmentDate = DateTime.Now.AddHours(6),
                            Status = "Confirmed"
                        },

                        new()
                        {
                            Id = 5,
                            PatientName = "Khaled Mahmoud",
                            DoctorName = "Dr. Nada Hassan",
                            ClinicName = "Dermatology Clinic",
                            AppointmentDate = DateTime.Now.AddDays(2),
                            Status = "Pending"
                        }
                    }       
            };

            return View(vm);
        }
       
    }
}
