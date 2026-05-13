using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Payments;

namespace Presentation.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: /Payment
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // GET: /Payment/PatientPayments/id
        public async Task<IActionResult> PatientPayments(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var payments = await _paymentService.GetPatientPaymentsAsync(id);
            return View(payments);
        }

        // GET: /Payment/DoctorPayments/id
        public async Task<IActionResult> DoctorPayments(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var payments = await _paymentService.GetDoctorPaymentsAsync(id);
            return View(payments);
        }

        // GET: /Payment/Status/status
        public async Task<IActionResult> Status(PaymentStatus status)
        {
            var payments = await _paymentService.GetPaymentsByStatusAsync(status);
            return View(payments);
        }

        // GET: /Payment/Details/id
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: /Payment/Create
        public IActionResult Create(int appointmentId = 0)
        {
            var vm = new PaymentCreateViewModel
            {
                AppointmentId = appointmentId,
                Status = PaymentStatus.Pending
            };

            return View(vm);
        }

        // POST: /Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var payment = new Payment
            {
                Amount = vm.Amount,
                PaymentMethod = vm.PaymentMethod,
                Status = vm.Status,
                AppointmentId = vm.AppointmentId,
                TransactionReference = vm.TransactionReference,
                FailureReason = vm.FailureReason,
                PaidAt = vm.Status == PaymentStatus.Completed ? DateTime.UtcNow : null
            };

            await _paymentService.AddPaymentAsync(payment);

            TempData["Success"] = "Payment added successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Payment/Edit/id
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            var vm = new PaymentEditViewModel
            {
                Id = payment.Id,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                Status = payment.Status,
                AppointmentId = payment.AppointmentId,
                TransactionReference = payment.TransactionReference,
                FailureReason = payment.FailureReason
            };

            return View(vm);
        }

        // POST: /Payment/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentEditViewModel vm)
        {
            if (id != vm.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var payment = new Payment
            {
                Id = vm.Id,
                Amount = vm.Amount,
                PaymentMethod = vm.PaymentMethod,
                Status = vm.Status,
                AppointmentId = vm.AppointmentId,
                TransactionReference = vm.TransactionReference,
                FailureReason = vm.FailureReason,
                PaidAt = vm.Status == PaymentStatus.Completed ? DateTime.UtcNow : null
            };

            await _paymentService.UpdatePaymentAsync(payment);

            TempData["Success"] = "Payment updated successfully";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Payment/Delete/id
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // POST: /Payment/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _paymentService.DeletePaymentAsync(id);

            TempData["Success"] = "Payment deleted successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}