using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.BankIntegration.Services
{
    public interface IPaymentsService
    {
        Task<bool> ProcessPaymentAsync(CardPaymentRequest cardPaymentRequest, CancellationToken cancellationToken = default);
    }
}
