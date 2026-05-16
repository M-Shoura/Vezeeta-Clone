using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Payments;
using System.Security.Claims;

namespace Presentation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentController(IPaymentService paymentService, IUnitOfWork unitOfWork)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
        }

        // GET: /Payment
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // GET: /Payment/DoctorPayments/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DoctorPayments(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var payments = await _paymentService.GetDoctorPaymentsAsync(id);
            return View(payments);
        }

        // GET: /Payment/Details/id
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: /Payment/Create
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create(int appointmentId = 0, bool autoCheckout = false)
        {
            if (autoCheckout && appointmentId > 0)
            {
                var checkoutResult = await CreateCheckoutForAppointmentAsync(appointmentId);
                if (checkoutResult is not null)
                {
                    return checkoutResult;
                }
            }

            if (appointmentId > 0)
            {
                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Forbid();

                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null)
                    return NotFound();

                if (appointment.PatientId != currentUserId)
                    return Forbid();

                var existingPayment = await _paymentService.GetPaymentByAppointmentIdAsync(appointmentId);
                if (existingPayment != null)
                {
                    var isSuccess = existingPayment.Status == PaymentStatus.Completed;
                    var message = isSuccess
                        ? "This appointment is already paid."
                        : "A payment already exists for this appointment.";

                    return RedirectToAction(nameof(Result), new
                    {
                        paymentId = existingPayment.Id,
                        isSuccess,
                        message
                    });
                }
            }

            var vm = new PaymentCreateViewModel
            {
                AppointmentId = appointmentId
            };

            return View(vm);
        }

        private async Task<IActionResult?> CreateCheckoutForAppointmentAsync(int appointmentId)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingPayment = await _paymentService.GetPaymentByAppointmentIdAsync(appointmentId);
            if (existingPayment != null)
            {
                if (!string.IsNullOrWhiteSpace(currentUserId))
                {
                    var currentPaymentAppointment = await _unitOfWork.Repository<Appointment>().FindAsync(
                        a => a.Id == appointmentId && a.PatientId == currentUserId,
                        includes: new[]
                        {
                            "Doctor.ApplicationUser",
                            "Patient.ApplicationUser",
                            "Clinic"
                        });

                    if (currentPaymentAppointment == null)
                    {
                        return Forbid();
                    }
                }

                return await RedirectToPaymobAsync(existingPayment.Id, appointmentId);
            }

            var appointment = await _unitOfWork.Repository<Appointment>().FindAsync(
                a => a.Id == appointmentId,
                includes: new[]
                {
                    "Doctor.ApplicationUser",
                    "Patient.ApplicationUser",
                    "Clinic"
                });

            if (appointment == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(currentUserId) || appointment.PatientId != currentUserId)
            {
                return Forbid();
            }

            var doctorClinic = await _unitOfWork.Repository<DoctorClinic>().FindAsync(
                dc => dc.DoctorId == appointment.DoctorId && dc.ClinicId == appointment.ClinicId,
                includes: new[] { "Doctor", "Clinic" });

            if (doctorClinic == null)
            {
                return BadRequest("No consultation fee is configured for this doctor and clinic.");
            }

            var payment = new Payment
            {
                Amount = doctorClinic.ConsultationFee,
                PaymentMethod = PaymentMethod.Visa,
                Status = PaymentStatus.Pending,
                AppointmentId = appointment.Id
            };

            await _paymentService.AddPaymentAsync(payment);

            return await RedirectToPaymobAsync(payment.Id, appointmentId);
        }

        private async Task<IActionResult> RedirectToPaymobAsync(int paymentId, int appointmentId)
        {
            var successUrl = Url.Action(
                action: nameof(Success),
                controller: "Payment",
                values: new { paymentId },
                protocol: Request.Scheme);

            var cancelUrl = Url.Action(
                action: nameof(Cancel),
                controller: "Payment",
                values: new { paymentId },
                protocol: Request.Scheme);

            if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
                return StatusCode(500);

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            var description = payment is null
                ? $"Appointment payment #{appointmentId}"
                : $"Appointment payment #{payment.AppointmentId}";

            var checkoutUrl = await _paymentService.CreatePaymobCheckoutUrlAsync(
                paymentId,
                successUrl,
                cancelUrl,
                description);

            return Redirect(checkoutUrl);
        }

        

        // POST: /Payment/Create
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Forbid();

            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(vm.AppointmentId);
            if (appointment == null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "Appointment not found.");
                return View(vm);
            }

            if (appointment.PatientId != currentUserId)
                return Forbid();

            var existingPayment = await _paymentService.GetPaymentByAppointmentIdAsync(vm.AppointmentId);
            if (existingPayment != null)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), "A payment already exists for this appointment.");
                return View(vm);
            }

            var payment = new Payment
            {
                Amount = vm.Amount,
                PaymentMethod = PaymentMethod.Visa,
                Status = PaymentStatus.Pending,
                AppointmentId = vm.AppointmentId
            };

            await _paymentService.AddPaymentAsync(payment);

            var successUrl = Url.Action(
                action: nameof(Success),
                controller: "Payment",
                values: new { paymentId = payment.Id },
                protocol: Request.Scheme);

            var cancelUrl = Url.Action(
                action: nameof(Cancel),
                controller: "Payment",
                values: new { paymentId = payment.Id },
                protocol: Request.Scheme);

            if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
                return StatusCode(500);

            var checkoutUrl = await _paymentService.CreatePaymobCheckoutUrlAsync(
                payment.Id,
                successUrl,
                cancelUrl,
                vm.Description);

            return Redirect(checkoutUrl);
        }

        // GET: /Payment/Success?paymentId=1&transactionId=...
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Success(int paymentId, string? transactionId = null)
        {
            if (!await IsPaymentOwnedByCurrentPatientAsync(paymentId))
                return Forbid();

            var completed = await _paymentService.FinalizePaymobPaymentAsync(paymentId, transactionId);
            if (!completed)
            {
                return RedirectToAction(nameof(Result), new
                {
                    paymentId,
                    isSuccess = false,
                    message = "Payment was not completed."
                });
            }

            return RedirectToAction(nameof(Result), new
            {
                paymentId,
                isSuccess = true,
                message = "Payment completed successfully."
            });
        }

        // GET: /Payment/Cancel?paymentId=1
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Cancel(int paymentId)
        {
            if (!await IsPaymentOwnedByCurrentPatientAsync(paymentId))
                return Forbid();

            return RedirectToAction(nameof(Result), new
            {
                paymentId,
                isSuccess = false,
                message = "Payment was canceled."
            });
        }

        // GET: /Payment/Result?paymentId=1&isSuccess=true
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Result(int paymentId, bool isSuccess, string? message = null)
        {
            if (!await IsPaymentOwnedByCurrentPatientAsync(paymentId))
                return Forbid();

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return NotFound();

            var vm = new PaymentResultViewModel
            {
                IsSuccess = isSuccess,
                Message = string.IsNullOrWhiteSpace(message)
                    ? (isSuccess ? "Payment completed successfully." : "Payment failed.")
                    : message,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                PaidAt = payment.PaidAt,
                AppointmentDate = payment.AppointmentDate,
                StartTime = payment.StartTime,
                EndTime = payment.EndTime,
                DoctorName = payment.DoctorName,
                ClinicName = payment.ClinicName
            };

            return View(vm);
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<bool> IsPaymentOwnedByCurrentPatientAsync(int paymentId)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return false;

            var payment = await _unitOfWork.Repository<Payment>().FindAsync(
                p => p.Id == paymentId && p.Appointment.PatientId == currentUserId,
                includes: new[] { "Appointment" });

            return payment != null;
        }
    }
}