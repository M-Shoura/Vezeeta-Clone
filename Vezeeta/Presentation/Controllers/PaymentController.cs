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
        public async Task<IActionResult> Create(int appointmentId = 0)
        {
            if (appointmentId > 0)
            {
                var existingPayment = await _paymentService.GetPaymentByAppointmentIdAsync(appointmentId);
                if (existingPayment != null)
                    return RedirectToAction(nameof(Details), new { id = existingPayment.Id });
            }

            var vm = new PaymentCreateViewModel
            {
                AppointmentId = appointmentId
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
        public async Task<IActionResult> Success(int paymentId, string? transactionId = null)
        {
            var completed = await _paymentService.FinalizePaymobPaymentAsync(paymentId, transactionId);
            if (!completed)
                return BadRequest("Payment was not completed.");

            TempData["Success"] = "Payment completed successfully";
            return RedirectToAction(nameof(Details), new { id = paymentId });
        }

        // GET: /Payment/Cancel?paymentId=1
        public IActionResult Cancel(int paymentId)
        {
            TempData["Error"] = "Payment was canceled.";
            return RedirectToAction(nameof(Details), new { id = paymentId });
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

            var currentPayment = await _paymentService.GetPaymentByIdAsync(id);
            if (currentPayment == null)
                return NotFound();

            var payment = new Payment
            {
                Id = vm.Id,
                Amount = vm.Amount,
                PaymentMethod = vm.PaymentMethod,
                Status = currentPayment.Status,
                AppointmentId = vm.AppointmentId,
                TransactionReference = vm.TransactionReference,
                FailureReason = vm.FailureReason,
                PaidAt = currentPayment.PaidAt
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