using Application.Interfaces.Services;
using Application.Interfaces.Repositories;
using Application.DTOs.Payments;
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

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DoctorPayments(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var payments = await _paymentService.GetDoctorPaymentsAsync(id);
            return View(payments);
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return payment == null ? NotFound() : View(payment);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create(int appointmentId = 0, bool autoCheckout = false)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return Forbid();

            if (appointmentId > 0)
            {
                var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);
                if (appointment == null || appointment.PatientId != currentUserId)
                    return appointment == null ? NotFound() : (IActionResult)Forbid();

                var existingPayment = await _paymentService.GetPaymentByAppointmentIdAsync(appointmentId);
                if (existingPayment != null)
                    return RedirectToAction(nameof(Result), CreateResultParams(existingPayment));

                if (autoCheckout)
                {
                    var checkoutResult = await ProcessAutoCheckoutAsync(appointmentId, currentUserId);
                    if (checkoutResult != null)
                        return checkoutResult;
                }
            }

            return View(new PaymentCreateViewModel { AppointmentId = appointmentId });
        }

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
            if (appointment == null || appointment.PatientId != currentUserId)
            {
                ModelState.AddModelError(nameof(vm.AppointmentId), appointment == null ? "Appointment not found." : "Unauthorized.");
                return View(vm);
            }

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
            return await RedirectToPaymobAsync(payment.Id, vm.Description);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Success(int paymentId, string? transactionId = null)
        {
            if (!await IsPaymentOwnedByCurrentPatientAsync(paymentId))
                return Forbid();

            var completed = await _paymentService.FinalizePaymobPaymentAsync(paymentId, transactionId);
            var resultParams = new { paymentId, isSuccess = completed, message = completed ? "Payment completed successfully." : "Payment was not completed." };
            return RedirectToAction(nameof(Result), resultParams);
        }

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Cancel(int paymentId)
        {
            if (!await IsPaymentOwnedByCurrentPatientAsync(paymentId))
                return Forbid();

            return RedirectToAction(nameof(Result), new { paymentId, isSuccess = false, message = "Payment was canceled." });
        }

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
                Message = message ?? (isSuccess ? "Payment completed successfully." : "Payment failed."),
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                PaidAt = payment.PaidAt,
                DoctorName = payment.DoctorName,
                ClinicName = payment.ClinicName
            };

            return View(vm);
        }

        private async Task<IActionResult?> ProcessAutoCheckoutAsync(int appointmentId, string currentUserId)
        {
            var doctorClinic = await GetAppointmentDoctorClinicAsync(appointmentId, currentUserId);
            if (doctorClinic == null)
                return BadRequest("No consultation fee is configured for this doctor and clinic.");

            var payment = new Payment
            {
                Amount = doctorClinic.ConsultationFee,
                PaymentMethod = PaymentMethod.Visa,
                Status = PaymentStatus.Pending,
                AppointmentId = appointmentId
            };

            await _paymentService.AddPaymentAsync(payment);
            return await RedirectToPaymobAsync(payment.Id, null);
        }

        private async Task<DoctorClinic?> GetAppointmentDoctorClinicAsync(int appointmentId, string currentUserId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().FindAsync(a => a.Id == appointmentId && a.PatientId == currentUserId);
            if (appointment == null)
                return null;

            return await _unitOfWork.Repository<DoctorClinic>().FindAsync(
                dc => dc.DoctorId == appointment.DoctorId && dc.ClinicId == appointment.ClinicId);
        }

        private async Task<IActionResult> RedirectToPaymobAsync(int paymentId, string? description)
        {
            var successUrl = GenerateUrl(nameof(Success), new { paymentId }) ?? throw new InvalidOperationException("Failed to generate success URL.");
            var cancelUrl = GenerateUrl(nameof(Cancel), new { paymentId }) ?? throw new InvalidOperationException("Failed to generate cancel URL.");

            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            description ??= $"Appointment payment #{payment?.AppointmentId ?? 0}";

            var checkoutUrl = await _paymentService.CreatePaymobCheckoutUrlAsync(paymentId, successUrl, cancelUrl, description);
            return Redirect(checkoutUrl);
        }

        private object CreateResultParams(PaymentDto payment)
        {
            var isSuccess = payment.Status == PaymentStatus.Completed;
            var message = isSuccess ? "This appointment is already paid." : "A payment already exists for this appointment.";
            return new { paymentId = payment.Id, isSuccess, message };
        }

        private string? GenerateUrl(string action, object values)
            => Url.Action(action: action, controller: "Payment", values: values, protocol: Request.Scheme);

        private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private async Task<bool> IsPaymentOwnedByCurrentPatientAsync(int paymentId)
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(currentUserId))
                return false;

            var payment = await _unitOfWork.Repository<Payment>().FindAsync(p => p.Id == paymentId && p.Appointment.PatientId == currentUserId, includes: new[] { "Appointment" });
            return payment != null;
        }
    }
}