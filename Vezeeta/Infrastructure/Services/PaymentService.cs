using Application.DTOs.Payments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            return await _paymentRepository.GetPaymentByIdAsync(paymentId);
        }

        public async Task<PaymentDto?> GetPaymentByAppointmentIdAsync(int appointmentId)
        {
            return await _paymentRepository.GetPaymentByAppointmentIdAsync(appointmentId);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllPaymentsAsync();
        }

        public async Task<IEnumerable<PaymentDto>> GetPatientPaymentsAsync(string patientId)
        {
            return await _paymentRepository.GetPatientPaymentsAsync(patientId);
        }

        public async Task<IEnumerable<PaymentDto>> GetDoctorPaymentsAsync(string doctorId)
        {
            return await _paymentRepository.GetDoctorPaymentsAsync(doctorId);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            return await _paymentRepository.GetPaymentsByStatusAsync(status);
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.UtcNow;

            if (payment.Status == PaymentStatus.Completed && payment.PaidAt == null)
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdatePaymentAsync(Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;

            if (payment.Status == PaymentStatus.Completed && payment.PaidAt == null)
            {
                payment.PaidAt = DateTime.UtcNow;
            }

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
            return payment;
        }

        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            _unitOfWork.Repository<Payment>().Delete(payment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<string> CreatePaymobCheckoutUrlAsync(int paymentId, string successUrl, string cancelUrl, string? description = null)
        {
            var apiKey = _configuration["Paymob:ApiKey"];
            var integrationIdValue = _configuration["Paymob:IntegrationId"];
            var iframeId = _configuration["Paymob:IframeId"];
            var currency = _configuration["Paymob:Currency"] ?? "EGP";

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(integrationIdValue) ||
                string.IsNullOrWhiteSpace(iframeId))
            {
                throw new InvalidOperationException("Paymob is not configured.");
            }

            if (!int.TryParse(integrationIdValue, out var integrationId))
                throw new InvalidOperationException("Paymob integration id is invalid.");

            var payment = await _unitOfWork.Repository<Payment>().FindAsync(
                p => p.Id == paymentId,
                new[] { "Appointment.Patient.ApplicationUser" });

            if (payment == null)
                throw new InvalidOperationException("Payment not found.");

            var httpClient = new HttpClient();
            var authToken = await CreateAuthTokenAsync(httpClient, apiKey);
            var orderId = await RegisterOrderAsync(httpClient, authToken, payment, currency, description);
            var paymentToken = await CreatePaymentKeyAsync(
                httpClient,
                authToken,
                orderId,
                payment,
                integrationId,
                currency,
                successUrl,
                cancelUrl,
                description);

            payment.TransactionReference = orderId.ToString();
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={Uri.EscapeDataString(paymentToken)}";
        }

        public async Task<bool> FinalizePaymobPaymentAsync(int paymentId, string? transactionId = null)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.TransactionReference = transactionId ?? payment.TransactionReference;
            payment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static async Task<string> CreateAuthTokenAsync(HttpClient httpClient, string apiKey)
        {
            var response = await httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new { api_key = apiKey });

            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("token").GetString()
                   ?? throw new InvalidOperationException("Paymob auth token was not returned.");
        }

        private static async Task<long> RegisterOrderAsync(HttpClient httpClient, string authToken, Payment payment, string currency, string? description)
        {
            var patient = payment.Appointment?.Patient?.ApplicationUser;
            var firstName = !string.IsNullOrWhiteSpace(patient?.FullName) ? patient!.FullName.Split(' ', 2)[0] : "Vezeeta";
            var lastName = !string.IsNullOrWhiteSpace(patient?.FullName) && patient!.FullName.Contains(' ')
                ? patient.FullName.Split(' ', 2)[1]
                : "Patient";

            var requestBody = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = ((long)(payment.Amount * 100)).ToString(),
                currency,
                items = Array.Empty<object>(),
                shipping_data = new
                {
                    apartment = "0",
                    email = patient?.Email ?? "payment@vezeeta.local",
                    floor = "0",
                    first_name = firstName,
                    street = description ?? "Vezeeta appointment payment",
                    building = "0",
                    phone_number = patient?.PhoneNumber ?? "01000000000",
                    shipping_method = "PKG",
                    postal_code = "00000",
                    city = "Cairo",
                    country = "EG",
                    last_name = lastName,
                    state = "Cairo"
                }
            };

            var response = await httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", requestBody);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("id").GetInt64();
        }

        private static async Task<string> CreatePaymentKeyAsync(
            HttpClient httpClient,
            string authToken,
            long orderId,
            Payment payment,
            int integrationId,
            string currency,
            string successUrl,
            string cancelUrl,
            string? description)
        {
            var patient = payment.Appointment?.Patient?.ApplicationUser;
            var firstName = !string.IsNullOrWhiteSpace(patient?.FullName) ? patient!.FullName.Split(' ', 2)[0] : "Vezeeta";
            var lastName = !string.IsNullOrWhiteSpace(patient?.FullName) && patient!.FullName.Contains(' ')
                ? patient.FullName.Split(' ', 2)[1]
                : "Patient";

            var requestBody = new
            {
                auth_token = authToken,
                amount_cents = ((long)(payment.Amount * 100)).ToString(),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "0",
                    email = patient?.Email ?? "payment@vezeeta.local",
                    floor = "0",
                    first_name = firstName,
                    street = description ?? "Vezeeta appointment payment",
                    building = "0",
                    phone_number = patient?.PhoneNumber ?? "01000000000",
                    shipping_method = "PKG",
                    postal_code = "00000",
                    city = "Cairo",
                    country = "EG",
                    last_name = lastName,
                    state = "Cairo"
                },
                currency,
                integration_id = integrationId,
                redirection_url = $"{successUrl}&transactionId={{TRANSACTION_ID}}",
                cancel_url = cancelUrl
            };

            var response = await httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", requestBody);
            response.EnsureSuccessStatusCode();

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("token").GetString()
                   ?? throw new InvalidOperationException("Paymob payment token was not returned.");
        }
    }
}